using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Nation = BackendAccountService.Core.Models.Nation;
using OrganisationType = BackendAccountService.Core.Models.OrganisationType;
using ProducerType = BackendAccountService.Core.Models.ProducerType;

namespace BackendAccountService.Core.UnitTests.Services.AccountServiceTests;

[TestClass]
public class AccountsServiceTests
{
    private AccountsDbContext _accountContext = null!;
    private Mock<ITokenService>? _tokenService;
    private AccountService _accountService = null!;
    private static readonly Guid UserGuid = Guid.NewGuid();
    private static readonly int UserId = 1234;
    private const string TestServiceRole = "TestServiceRole";

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _accountContext = new AccountsDbContext(contextOptions);

        _tokenService = new Mock<ITokenService>();

        _accountService = new AccountService(_accountContext, _tokenService.Object, new Mock<IReExEnrolmentMaps>().Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _accountContext.Dispose();
    }

    [TestMethod]
    public async Task WhenQueryForExistingServiceRole_ThenReturnIt()
    {
        //Act
        var serviceRole = await _accountService.GetServiceRoleAsync(TestServiceRole);

        //Assert
        serviceRole.Should().NotBeNull();
        serviceRole?.Id.Should().BeGreaterThan(0);
        serviceRole?.Name.Should().Be(TestServiceRole);
    }

    [TestMethod]
    public async Task WhenQueryForNotExistingServiceRole_ThenReturnNull()
    {
        //Act
        var serviceRole = await _accountService.GetServiceRoleAsync("NoneExistentServiceRole");

        //Assert
        serviceRole.Should().BeNull();
    }

    [TestMethod]
    public async Task WhenQueryForOrganisationSavedAsPartOfEnrolment_ThenReturnIt()
    {
        //Setup
        var accountToCreate = GetAccountModel();

        var serviceRole = new ServiceRole { Id = 1 };

        //Act
        await _accountService.AddAccountAsync(accountToCreate, serviceRole);

        //Assert
        _accountContext.Enrolments
            .FirstOrDefault(enrolment => enrolment.Connection.Organisation.Name == accountToCreate.Organisation.Name)
            .Should().NotBeNull();
    }

    [TestMethod]
    public async Task AddReprocessorExporterAccountAsync_WhenValidReprocessorExporterAccount_ThenAddsMappedPersonToContext()
    {
        //Setup
        const string firstName = "FirstName";
        const string lastName = "LastName";
        const string email = "E6ABF976-6C1B-48B8-BFB9-C2BE69406233@example.com";
        const string telephone = "01234567890";
        Guid userId = Guid.NewGuid();

        var account = new ReprocessorExporterAccount
        {
            Person = new PersonModel
            {
                FirstName = firstName,
                LastName = lastName,
                ContactEmail = email,
                TelephoneNumber = telephone
            },
            User = new UserModel
            {
                UserId = userId,
                ExternalIdpId = null,
                ExternalIdpUserId = null,
                Email = email
            }
        };

        //Act
        await _accountService.AddReprocessorExporterAccountAsync(account, "service", userId);

        //Assert
        var addedPerson = _accountContext.Persons
            .FirstOrDefault(person => person.Email == email);

        // this basically duplicates the PersonMapper test, which is not good.
        // the mapper should really be DI rather than static, but it's a static for consistency with existing code
        addedPerson.Should().NotBeNull();
        addedPerson!.Telephone.Should().Be(telephone);
        addedPerson.FirstName.Should().Be(firstName);
        addedPerson.LastName.Should().Be(lastName);
        addedPerson.Email.Should().Be(email);
        addedPerson.IsDeleted.Should().BeFalse();
        addedPerson.User.Should().NotBeNull();
        addedPerson.User!.UserId.Should().Be(userId);

        addedPerson.UserId.Should().BeGreaterThanOrEqualTo(0);
        addedPerson.OrganisationConnections.Should().BeNull();
        addedPerson.FromPersonConnections.Should().BeNull();
        addedPerson.ToPersonConnections.Should().BeNull();
        addedPerson.RegulatorComments.Should().BeNull();
    }

    [TestMethod]
    public async Task AddReprocessorExporterAccountAsync_WhenValidReprocessorExporterAccount_ThenCorrectAuditLogsAreAdded()
    {
        //Setup
        const string serviceKey = "ServiceKey";
        const string firstName = "FirstName";
        const string lastName = "LastName";
        const string email = "E6ABF976-6C1B-48B8-BFB9-C2BE69406233@example.com";
        const string telephone = "01234567890";
        Guid userId = Guid.NewGuid();

        var account = new ReprocessorExporterAccount
        {
            Person = new PersonModel
            {
                FirstName = firstName,
                LastName = lastName,
                ContactEmail = email,
                TelephoneNumber = telephone
            },
            User = new UserModel
            {
                UserId = userId,
                ExternalIdpId = null,
                ExternalIdpUserId = null,
                Email = email
            }
        };

        //Act
        await _accountService.AddReprocessorExporterAccountAsync(account, serviceKey, userId);

        //Assert
        var personAuditLog = _accountContext.AuditLogs
            .FirstOrDefault(l => l.UserId == userId && l.ServiceId == serviceKey && l.Entity == "Person");

        personAuditLog.Should().NotBeNull();

        var userAuditLog = _accountContext.AuditLogs
            .FirstOrDefault(l => l.UserId == userId && l.ServiceId == serviceKey && l.Entity == "User");

        userAuditLog.Should().NotBeNull();
    }

    [TestMethod]
    public async Task AddApprovedUserAccount_CanNotProcessNullQuery()
    {
        //Setup
        var street = "some street";
        var account = new ApprovedUserAccountModel
        {
            Connection = new ConnectionModel(),
            Person = new PersonModel
            {
                ContactEmail = "cake@cake.com",
                FirstName = "Cake",
                LastName = "Cake",
                TelephoneNumber = "02123"
            },
            Organisation = new OrganisationModel
            {
                Name = "cake",
                Address = new AddressModel
                {
                    Street = street
                }
            }
        };

        //Act
        var test = async () => await _accountService.AddApprovedUserAccountAsync(
            account,
            new ServiceRole(),
            new UserModel
            {
                UserId = Guid.NewGuid(),
                Email = "testUser@test.com",
                ExternalIdpUserId = "12345",
                ExternalIdpId = "oneTwoThree",
                Id = 1
            });

        //Assert
        await test.Should().ThrowAsync<InvalidOperationException>();
    }

    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

        var service = new Service
        {
            Name = "TestService",
            Description = "Test service",
            Key = "TestService",
            ServiceRoles = new List<ServiceRole>
            {
                new ()
                {
                    Name = TestServiceRole,
                    Key = TestServiceRole
                }
            }
        };
        setupContext.Services.Add(service);

        var user = new User
        {
            UserId = UserGuid,
            Id = UserId
        };
        setupContext.Users.Add(user);

        const int personId = 456;
        var person = new Person
        {
            UserId = UserId,
            Email = "someEmail@test.com",
            FirstName = "TestFirstName",
            LastName = "TestLastName",
            Telephone = "0123456789",
            Id = personId
        };
        setupContext.Persons.Add(person);

        const int connectionId = 789;
        var personOrganisationConnections = new PersonOrganisationConnection
        {
            PersonId = personId,
            Id = connectionId,

        };
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnections);

        var enrolment = new Enrolment
        {
            ConnectionId = connectionId,
            ServiceRole = service.ServiceRoles.First()
        };
        setupContext.Enrolments.Add(enrolment);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    private static AccountModel GetAccountModel()
    {
        return new AccountModel
        {
            Connection = new ConnectionModel
            {
                ServiceRole = TestServiceRole,
                JobTitle = "Job Title"
            },
            Organisation = new OrganisationModel
            {
                Address = new AddressModel
                {
                    SubBuildingName = "Sub-building",
                    BuildingName = "Building",
                    BuildingNumber = "123-125",
                    Street = "Street",
                    Locality = "Locality",
                    DependentLocality = "Dependent-Locality",
                    County = "Test County",
                    Country = "Northern Ireland",
                    Postcode = "BT48 123",
                    Town = "SomeTown"
                },
                CompaniesHouseNumber = "12345",
                Name = "Test company one",
                OrganisationType = OrganisationType.CompaniesHouseCompany,
                ProducerType = ProducerType.NotSet,
                Nation = Nation.NotSet
            },
            Person = new PersonModel
            {
                ContactEmail = "test@test.com",
                FirstName = "Johnny",
                LastName = "Cash",
                TelephoneNumber = "07905606060",
            },
            User = new UserModel
            {
                UserId = Guid.NewGuid(),
                Email = "test@test.com",
                ExternalIdpId = null!,
                ExternalIdpUserId = null!,
                Id = 1
            }
        };
    }
}
