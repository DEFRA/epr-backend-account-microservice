
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BackendAccountService.Core.Models;
using OrganisationType = BackendAccountService.Core.Models.OrganisationType;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class AccountsServiceTests
{
    private AccountsDbContext _accountContext= null!;
    private AccountService _accountService = null!;

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _accountContext = new AccountsDbContext(contextOptions);
        _accountService = new AccountService(_accountContext);
    }

    [TestMethod]
    public async Task WhenQueryForExistingServiceRole_ThenReturnIt()
    {
        //Act
        var serviceRole = await _accountService.GetServiceRoleAsync("TestServiceRole");
		
		//Assert
		serviceRole.Should().NotBeNull();
        serviceRole?.Id.Should().BeGreaterThan(0);
        serviceRole?.Name.Should().Be("TestServiceRole");
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
                UserId = Guid.NewGuid(), Email = "testUser@test.com", ExternalIdpUserId = "12345",
                ExternalIdpId = "oneTwoThree",Id = 1
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
                    Name = "TestServiceRole",
                    Key = "TestServiceRole"
                }
            }
        };

        setupContext.Services.Add(service);
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
    
    private static AccountModel GetAccountModel()
    {
        return new AccountModel
        {
            Connection = new ConnectionModel
            {
                ServiceRole = "TestServiceRole",
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
                ProducerType = Models.ProducerType.NotSet
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
