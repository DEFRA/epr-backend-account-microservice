using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Controllers;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;
using Nation = BackendAccountService.Core.Models.Nation;
using OrganisationType = BackendAccountService.Core.Models.OrganisationType;
using ProducerType = BackendAccountService.Core.Models.ProducerType;

namespace BackendAccountService.Data.IntegrationTests.Controllers;

/// <summary>
/// ReprocessorExporterAccountsController integration tests.
/// </summary>
/// <remarks>
/// We can't have the luxury of fully comprehensive, single check integration tests, like we can with unit tests,
/// as the integration tests take longer to run, so we cover the important aspects and sometimes combine multiple checks into a single test.
/// </remarks>
[TestClass]
public class ReprocessorExporterAccountsControllerTests
{
    private const string AuditServiceName = "Integration Test";

    private static AzureSqlDbContainer? _database;
    private static AccountsDbContext _context = null!;
    private static ReprocessorExporterAccountsController _controller = null!;

    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        try
        {
            _database = await AzureSqlDbContainer.StartDockerDbAsync();

            _context = new AccountsDbContext(
                new DbContextOptionsBuilder<AccountsDbContext>()
                    .UseSqlServer(_database.ConnectionString)
                    .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .Options);

            await _context.Database.MigrateAsync();

            Mock<IOptions<ApiConfig>> apiConfigOptionsMock = new();

            apiConfigOptionsMock
                .Setup(x => x.Value)
                .Returns(new ApiConfig
                {
                    BaseProblemTypePath = "https://epr-errors/"
                });

            var tokenService = new Mock<ITokenService>();

            tokenService.Setup(s => s.GenerateInviteToken())
                .Returns("test-invite-token");

            _controller = new ReprocessorExporterAccountsController(
                new AccountService(_context, tokenService.Object, new ReExEnrolmentMaps()),
                new PersonService(_context),
                new PartnerService(_context),
                new OrganisationService(_context),
                apiConfigOptionsMock.Object);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static async Task TestFixtureTearDown()
    {
        if (_database == null)
        {
            return;
        }
        await _database.StopAsync();
    }

    [TestMethod]
    public async Task CreateAccount_HappyPath_ReturnsOk()
    {
        var user = GetTestUser();

        var reprocessorExporterAccount = new ReprocessorExporterAccount
        {
            Person = new PersonModel
            {
                ContactEmail = "inttest@example.com",
                FirstName = "Bob",
                LastName = "Smith",
                TelephoneNumber = "01234567890"
            },
            User = new UserModel
            {
                UserId = user.id,
                Email = "inttest@example.com"
            }
        };

        var result = await _controller.CreateAccount(
            reprocessorExporterAccount,
            AuditServiceName,
            user.id);

        var okResponse = result as OkResult;
        okResponse.Should().NotBeNull();
    }

    [TestMethod]
    public async Task AddOrganisation_HappyPath_ReturnsOkWithReExAddOrganisationResponse()
    {
        // Arrange
        var user = GetTestUser();

        await AddPerson(user.id, user.email);

        var reprocessorExporterAddOrganisation = new ReprocessorExporterAddOrganisation
        {
            User = new()
            {
                IsApprovedUser = true,
                JobTitle = "Director",
                UserId = user.id
            },
            Organisation = GetTestReprocessorExporterOrganisationModel(),
            Partners =
            [
                new()
                {
                    Name = "Mr Partner",
                    PartnerRole = "Corporate Partner"
                }
            ],
            InvitedApprovedUsers =
            [
                new()
                {
                    Person = new PersonModel
                    {
                        ContactEmail = $"{Guid.NewGuid()}@example.com",
                        FirstName = "Invited",
                        LastName = "User",
                        TelephoneNumber = "01234567890"
                    },
                    JobTitle = "Director"
                }
            ],
            DeclarationTimeStamp = DateTime.UtcNow
        };

        // Act
        var result = await _controller.AddOrganisation(
            reprocessorExporterAddOrganisation,
            AuditServiceName,
            user.id);

        // Assert
        var okResponse = result as OkObjectResult;
        okResponse.Should().NotBeNull();

        var response = okResponse.Value as ReExAddOrganisationResponse;
        response.Should().NotBeNull();

        var assertContext = CreateAssertContext();

        // Find the organisation by name (should be unique due to test data)
        var dbOrganisation = await assertContext.Organisations
            .AsNoTracking()
            .Include(organisation => organisation.Nation)
            .Include(organisation => organisation.OrganisationType)
            .Include(organisation => organisation.ProducerType)
            .FirstOrDefaultAsync(o => o.Name == reprocessorExporterAddOrganisation.Organisation.Name);

        dbOrganisation.Should().NotBeNull();
        dbOrganisation!.Name.Should().Be(reprocessorExporterAddOrganisation.Organisation.Name);
        dbOrganisation.TradingName.Should().Be(reprocessorExporterAddOrganisation.Organisation.TradingName);
    }

    /// <summary>
    /// Add non-Uk organisation.
    /// </summary>
    /// <remarks>
    /// Not too useful as an integration test, as it's basically just the happy path,
    /// but useful for documentation and initially testing non-uk support.
    /// It'll be a candidate for removal at some point, as we don't want too many integration tests
    /// </remarks>
    [TestMethod]
    public async Task AddOrganisation_NonUk_ReturnsOkWithReExAddOrganisationResponse()
    {
        // Arrange
        var user = GetTestUser();

        await AddPerson(user.id, user.email);

        var organisation = GetTestReprocessorExporterOrganisationModel();

        // non-uk will have the country of the organisation, and the nation will be set to the chosen regulator nation
        organisation.Address.Country = "Réunion";
        organisation.Nation = Nation.Wales;
        organisation.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
        organisation.ProducerType = ProducerType.NonUkOrganisation;

        var reprocessorExporterAddOrganisation = new ReprocessorExporterAddOrganisation
        {
            User = new()
            {
                IsApprovedUser = true,
                // user's role is entered as free text in the non-uk journey
                JobTitle = "Free text role",
                UserId = user.id
            },
            Organisation = organisation,
            Partners = [],
            InvitedApprovedUsers =
            [
                new()
                {
                    Person = new PersonModel
                    {
                        ContactEmail = $"{Guid.NewGuid()}@example.com",
                        FirstName = "Invited",
                        LastName = "User",
                        TelephoneNumber = "01234567890"
                    },
                    JobTitle = "Manager or controller"
                }
            ],
            DeclarationTimeStamp = DateTime.UtcNow
        };

        // Act
        var result = await _controller.AddOrganisation(
            reprocessorExporterAddOrganisation,
            AuditServiceName,
            user.id);

        // Assert
        var okResponse = result as OkObjectResult;
        okResponse.Should().NotBeNull();

        var response = okResponse.Value as ReExAddOrganisationResponse;
        response.Should().NotBeNull();

        var assertContext = CreateAssertContext();

        // Find the organisation by name (should be unique due to test data)
        var dbOrganisation = await assertContext.Organisations
            .AsNoTracking()
            .Include(organisation => organisation.Nation)
            .Include(organisation => organisation.OrganisationType)
            .Include(organisation => organisation.ProducerType)
            .FirstOrDefaultAsync(o => o.Name == organisation.Name);

        dbOrganisation.Should().NotBeNull();
        dbOrganisation!.Country.Should().Be("Réunion");
        dbOrganisation.Nation.Id.Should().Be((int)Nation.Wales);
        dbOrganisation.OrganisationType.Id.Should().Be((int)OrganisationType.NonCompaniesHouseCompany);
        dbOrganisation.ProducerType.Id.Should().Be((int)ProducerType.NonUkOrganisation);
    }

    [TestMethod]
    public async Task AddOrganisation_UserIsApprovedUserNoPartnersNoInvitedUsers_CorrectEnrolmentsAdded()
    {
        // Arrange
        var user = GetTestUser();

        await AddPerson(user.id, user.email);

        var reprocessorExporterAddOrganisation = new ReprocessorExporterAddOrganisation
        {
            User = new()
            {
                IsApprovedUser = true,
                JobTitle = "Director",
                UserId = user.id
            },
            Organisation = GetTestReprocessorExporterOrganisationModel(),
            Partners = [],
            InvitedApprovedUsers = [],
            DeclarationTimeStamp = DateTime.UtcNow
        };

        // Act
        var result = await _controller.AddOrganisation(
            reprocessorExporterAddOrganisation,
            AuditServiceName,
            user.id);

        // Assert
        var okResponse = result as OkObjectResult;
        okResponse.Should().NotBeNull();

        var response = okResponse.Value as ReExAddOrganisationResponse;
        response.Should().NotBeNull();

        var assertContext = CreateAssertContext();

        List<Enrolment> userEnrolments = await assertContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection)
                .ThenInclude(c => c.Person)
            .Include(e => e.Connection)
            .Where(e => e.Connection != null && e.Connection.Person != null && e.Connection.Person.Email == user.email)
            .ToListAsync();

        // assert enrolment properties
        userEnrolments.Should().NotBeNull();
        userEnrolments.Should().HaveCount(2);
        var adminEnrolment = userEnrolments.SingleOrDefault(e =>
            e.ServiceRoleId == DbConstants.ServiceRole.ReprocessorExporter.AdminUser.Id);
        adminEnrolment.Should().NotBeNull();
        adminEnrolment!.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Enrolled);

        int organisationId = adminEnrolment.Connection.OrganisationId;

        // assert there are no partners for the organisation
        var organisationToPartnerRoles = await assertContext.OrganisationToPartnerRoles
            .AsNoTracking()
            .Where(o => o.OrganisationId == organisationId)
            .ToListAsync();
        organisationToPartnerRoles.Should().BeEmpty();

        // assert there are no invited approved users
        List<Enrolment> organisationEnrolments = await assertContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection)
            .Where(e => e.Connection != null && e.Connection.OrganisationId == organisationId)
            .ToListAsync();

        organisationEnrolments.Should().HaveCount(2);

        var approvedPersonEnrolment = userEnrolments.SingleOrDefault(e =>
            e.ServiceRoleId == DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id);
        approvedPersonEnrolment.Should().NotBeNull();
        approvedPersonEnrolment!.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Enrolled);
    }

    [TestMethod]
    public async Task AddOrganisation_UserIsNotApprovedUserMultiplePartnersNoInvitedUsers_AddsCorrectEnrolment()
    {
        // Arrange
        var user = GetTestUser();

        await AddPerson(user.id, user.email);

        string companyPartnerName = $"Corporate Partner {Guid.NewGuid()}";
        string individualPartnerName = $"Individual Partner {Guid.NewGuid()}";

        var reprocessorExporterAddOrganisation = new ReprocessorExporterAddOrganisation
        {
            User = new()
            {
                IsApprovedUser = false,
                JobTitle = "Director",
                UserId = user.id
            },
            Organisation = GetTestReprocessorExporterOrganisationModel(),
            Partners = [
                new()
                {
                    Name = companyPartnerName,
                    PartnerRole = "Corporate Partner"
                },
                new()
                {
                    Name = individualPartnerName,
                    PartnerRole = "Individual Partner"
                }
            ],
            InvitedApprovedUsers = [],
            DeclarationTimeStamp = DateTime.UtcNow
        };

        // Act
        var result = await _controller.AddOrganisation(
            reprocessorExporterAddOrganisation,
            AuditServiceName,
            user.id);

        // Assert
        var okResponse = result as OkObjectResult;
        okResponse.Should().NotBeNull();

        var response = okResponse.Value as ReExAddOrganisationResponse;
        response.Should().NotBeNull();

        var assertContext = CreateAssertContext();

        List<Enrolment> userEnrolments = await assertContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection)
                .ThenInclude(c => c.Person)
            .Include(e => e.Connection)
            .Where(e => e.Connection != null && e.Connection.Person != null && e.Connection.Person.Email == user.email)
            .ToListAsync();

        //todo: helper to get org id?
        userEnrolments.Should().NotBeNull();
        userEnrolments.Should().HaveCount(1);
        var userEnrolment = userEnrolments.First();
        int organisationId = userEnrolment.Connection.OrganisationId;

        // assert partners were added
        var organisationToPartnerRoles = await assertContext.OrganisationToPartnerRoles
            .AsNoTracking()
            .Where(o => o.OrganisationId == organisationId)
            .ToListAsync();
        organisationToPartnerRoles.Should().BeEquivalentTo(new List<OrganisationToPartnerRole>
            {
                new()
                {
                    Name = individualPartnerName,
                    OrganisationId = organisationId,
                    PartnerRoleId = DbConstants.PartnerRoleIds.IndividualPartner
                },
                new()
                {
                    Name = companyPartnerName,
                    OrganisationId = organisationId,
                    PartnerRoleId = DbConstants.PartnerRoleIds.CompanyPartner
                }
            },
            config => config
                .Excluding(o => o.Id)
                .Excluding(o => o.PartnerRole)
                .Excluding(o => o.Organisation));

        // assert they all have a valid id
        organisationToPartnerRoles.Should().AllSatisfy(e => e.Id.Should().BeGreaterThan(0));
    }

    [TestMethod]
    public async Task AddOrganisation_UserIsNotApprovedUserNoPartnersMultipleInvitedUsers_CreatesCorrectEntities()
    {
        // Arrange
        string invitedApprovedUserEmail1 = $"invited-1-{Guid.NewGuid()}@example.com";
        string invitedApprovedUserEmail2 = $"invited-2-{Guid.NewGuid()}@example.com";

        var user = GetTestUser();

        await AddPerson(user.id, user.email);

        var reprocessorExporterAddOrganisation = new ReprocessorExporterAddOrganisation
        {
            User = new()
            {
                IsApprovedUser = false,
                JobTitle = "Director",
                UserId = user.id
            },
            Organisation = GetTestReprocessorExporterOrganisationModel(),
            Partners = [],
            InvitedApprovedUsers =
            [
                new()
                {
                    Person = new PersonModel
                    {
                        ContactEmail = invitedApprovedUserEmail1,
                        FirstName = "Invited1",
                        LastName = "User1",
                        TelephoneNumber = "01234567890"
                    },
                    JobTitle = "Director"
                },
                new()
                {
                    Person = new PersonModel
                    {
                        ContactEmail = invitedApprovedUserEmail2,
                        FirstName = "Invited2",
                        LastName = "User2",
                        TelephoneNumber = "01234567891"
                    },
                    JobTitle = "Secretary"
                }
            ],
            DeclarationTimeStamp = DateTime.UtcNow
        };

        // Act
        var result = await _controller.AddOrganisation(
            reprocessorExporterAddOrganisation,
            AuditServiceName,
            user.id);

        // Assert
        var okResponse = result as OkObjectResult;
        okResponse.Should().NotBeNull();

        var response = okResponse.Value as ReExAddOrganisationResponse;
        response.Should().NotBeNull();

        var assertContext = CreateAssertContext();

        List<Enrolment> userEnrolments = await assertContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection)
                .ThenInclude(c => c.Person)
            .Include(e => e.Connection)
            .Where(e => e.Connection != null && e.Connection.Person != null && e.Connection.Person.Email == user.email)
            .ToListAsync();

        userEnrolments.Should().NotBeNull();
        userEnrolments.Should().HaveCount(1);
        var userEnrolment = userEnrolments.First();
        int organisationId = userEnrolment.Connection.OrganisationId;

        // assert invited persons were added
        var nonEnrolledEnrolments = await assertContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection)
            .ThenInclude(c => c.Person)
            .ThenInclude(p => p.User)
            .Where(e => e.EnrolmentStatusId != DbConstants.EnrolmentStatus.Enrolled
                        && e.Connection != null && e.Connection.OrganisationId == organisationId)
            .ToListAsync();

        nonEnrolledEnrolments.Should().BeEquivalentTo(new List<Enrolment>
        {
            new()
            {
                ServiceRoleId = DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
                EnrolmentStatusId = DbConstants.EnrolmentStatus.Invited,
                Connection = new PersonOrganisationConnection
                {
                    JobTitle = "Director",
                    OrganisationRoleId = DbConstants.OrganisationRole.Employer,
                    Person = new Person
                    {
                        Email = invitedApprovedUserEmail1,
                        FirstName = "Invited1",
                        LastName = "User1",
                        Telephone = "01234567890",
                        User = new User
                        {
                            UserId = Guid.Empty,
                            Email = invitedApprovedUserEmail1
                        }
                    },
                    PersonRoleId = DbConstants.PersonRole.Employee,
                    OrganisationId = organisationId
                }
            },
            new()
            {
                ServiceRoleId = DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
                EnrolmentStatusId = DbConstants.EnrolmentStatus.Invited,
                Connection = new PersonOrganisationConnection
                {
                    JobTitle = "Secretary",
                    OrganisationRoleId = DbConstants.OrganisationRole.Employer,
                    PersonRoleId = DbConstants.PersonRole.Employee,
                    OrganisationId = organisationId,
                    Person = new Person
                    {
                        Email = invitedApprovedUserEmail2,
                        FirstName = "Invited2",
                        LastName = "User2",
                        Telephone = "01234567891",
                        User = new User
                        {
                            UserId = Guid.Empty,
                            Email = invitedApprovedUserEmail2
                        }
                    }
                }
            }
        },
        config => config
            .Including(e => e.ServiceRoleId)
            .Including(e => e.EnrolmentStatusId)
            .Including(e => e.Connection.JobTitle)
            .Including(e => e.Connection.OrganisationRoleId)
            .Including(e => e.Connection.PersonRoleId)
            .Including(e => e.Connection.OrganisationId)
            .Including(e => e.Connection.Person.Email)
            .Including(e => e.Connection.Person.FirstName)
            .Including(e => e.Connection.Person.LastName)
            .Including(e => e.Connection.Person.Telephone)
            .Including(e => e.Connection.Person.User.UserId)
            .Including(e => e.Connection.Person.User.Email));

        // assert they all have a valid id
        nonEnrolledEnrolments.Should().AllSatisfy(e => e.Id.Should().BeGreaterThan(0));

        var personOrganisationConnectionInvites =
            await assertContext.PersonOrganisationConnectionInvites
                .AsNoTracking()
                .Include(poci => poci.Person)
                .Include(poci => poci.User)
                .Include(poci => poci.Organisation)
                .Where(poci => poci.Person.Email == invitedApprovedUserEmail1 || poci.Person.Email == invitedApprovedUserEmail2)
                .ToListAsync();

        personOrganisationConnectionInvites.Should().BeEquivalentTo(new List<PersonOrganisationConnectionInvite>
        {
            new()
            {
                InviteToken = "test-invite-token",
                Organisation = personOrganisationConnectionInvites.First(x => x.Person.Email == invitedApprovedUserEmail1).Organisation, // actual org instance
                Person = new Person
                {
                    Email = invitedApprovedUserEmail1,
                    FirstName = "Invited1",
                    LastName = "User1",
                    Telephone = "01234567890"
                },
                User = new User
                {
                    UserId = user.id,
                    Email = user.email,
                }
            },
            new()
            {
                InviteToken = "test-invite-token",
                Organisation = personOrganisationConnectionInvites.First(x => x.Person.Email == invitedApprovedUserEmail2).Organisation, // actual org instance
                Person = new Person
                {
                    Email = invitedApprovedUserEmail2,
                    FirstName = "Invited2",
                    LastName = "User2",
                    Telephone = "01234567891"
                },
                User = new User
                {
                    UserId = user.id,
                    Email = user.email,
                }
            }
        },
        config => config
            .Including(x => x.InviteToken)
            .Including(x => x.Organisation.Id)
            .Including(x => x.Organisation.Name)
            .Including(x => x.Person.Email)
            .Including(x => x.Person.FirstName)
            .Including(x => x.Person.LastName)
            .Including(x => x.Person.Telephone)
            .Including(x => x.User.UserId)
            .Including(x => x.Person.User.Email)
        );
    }

    [TestMethod]
    public async Task AddOrganisation_UserIsNotApprovedUserNoPartnersInvitedUserAlreadyExists_CreatesCorrectEntities()
    {
        // Arrange
        Guid invitedApprovedExistingUserId = Guid.NewGuid();
        string invitedApprovedExistingUserEmail = $"existing-invited-{Guid.NewGuid()}@example.com";
 
        var user = GetTestUser();

        // add the main user
        await AddPerson(user.id, user.email);

        // add the invited user
        await AddPerson(invitedApprovedExistingUserId, invitedApprovedExistingUserEmail);

        var reprocessorExporterAddOrganisation = new ReprocessorExporterAddOrganisation
        {
            User = new()
            {
                IsApprovedUser = false,
                JobTitle = "Director",
                UserId = user.id
            },
            Organisation = GetTestReprocessorExporterOrganisationModel(),
            Partners = [],
            InvitedApprovedUsers =
            [
                new()
                {
                    Person = new PersonModel
                    {
                        ContactEmail = invitedApprovedExistingUserEmail,
                        FirstName = "Existing",
                        LastName = "User",
                        TelephoneNumber = "01234567890"
                    },
                    JobTitle = "Burger flipper"
                }
            ],
            DeclarationTimeStamp = DateTime.UtcNow
        };

        // Act
        var result = await _controller.AddOrganisation(
            reprocessorExporterAddOrganisation,
            AuditServiceName,
            user.id);

        // Assert
        var okResponse = result as OkObjectResult;
        okResponse.Should().NotBeNull();

        var response = okResponse.Value as ReExAddOrganisationResponse;
        response.Should().NotBeNull();

        var assertContext = CreateAssertContext();

        List<Enrolment> userEnrolments = await assertContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection)
                .ThenInclude(c => c.Person)
            .Include(e => e.Connection)
            .Where(e => e.Connection != null && e.Connection.Person != null && e.Connection.Person.Email == user.email)
            .ToListAsync();

        userEnrolments.Should().NotBeNull();
        userEnrolments.Should().HaveCount(1);
        var userEnrolment = userEnrolments.First();
        int organisationId = userEnrolment.Connection.OrganisationId;

        // assert invited persons were added
        var nonEnrolledEnrolments = await assertContext.Enrolments
            .AsNoTracking()
            .Include(e => e.Connection)
            .ThenInclude(c => c.Person)
            .ThenInclude(p => p.User)
            .Where(e => e.EnrolmentStatusId != DbConstants.EnrolmentStatus.Enrolled
                        && e.Connection != null && e.Connection.OrganisationId == organisationId)
            .ToListAsync();

        nonEnrolledEnrolments.Should().BeEquivalentTo(new List<Enrolment>
        {
            new()
            {
                ServiceRoleId = DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
                EnrolmentStatusId = DbConstants.EnrolmentStatus.Invited,
                Connection = new PersonOrganisationConnection
                {
                    JobTitle = "Burger flipper",
                    OrganisationRoleId = DbConstants.OrganisationRole.Employer,
                    Person = new Person
                    {
                        Email = invitedApprovedExistingUserEmail,
                        //todo: should we update the existing person?
                        FirstName = "FirstName",
                        LastName = "LastName",
                        Telephone = "01234567890",
                        User = new User
                        {
                            UserId = invitedApprovedExistingUserId,
                            Email = invitedApprovedExistingUserEmail
                        }
                    },
                    PersonRoleId = DbConstants.PersonRole.Employee,
                    OrganisationId = organisationId
                }
            }
        },
        config => config
            .Including(e => e.ServiceRoleId)
            .Including(e => e.EnrolmentStatusId)
            .Including(e => e.Connection.JobTitle)
            .Including(e => e.Connection.OrganisationRoleId)
            .Including(e => e.Connection.PersonRoleId)
            .Including(e => e.Connection.OrganisationId)
            .Including(e => e.Connection.Person.Email)
            .Including(e => e.Connection.Person.FirstName)
            .Including(e => e.Connection.Person.LastName)
            .Including(e => e.Connection.Person.Telephone)
            .Including(e => e.Connection.Person.User.UserId)
            .Including(e => e.Connection.Person.User.Email));

        // assert they all have a valid id
        nonEnrolledEnrolments.Should().AllSatisfy(e => e.Id.Should().BeGreaterThan(0));

        var personOrganisationConnectionInvites =
            await assertContext.PersonOrganisationConnectionInvites
                .AsNoTracking()
                .Include(poci => poci.Person)
                .Include(poci => poci.User)
                .Include(poci => poci.Organisation)
                .Where(poci => poci.Person.Email == invitedApprovedExistingUserEmail)
                .ToListAsync();

        personOrganisationConnectionInvites.Should().BeEquivalentTo(new List<PersonOrganisationConnectionInvite>
        {
            new()
            {
                InviteToken = "test-invite-token",
                Organisation = personOrganisationConnectionInvites.First(x => x.Person.Email == invitedApprovedExistingUserEmail).Organisation, // actual org instance
                Person = new Person
                {
                    Email = invitedApprovedExistingUserEmail,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    Telephone = "01234567890"
                },
                User = new User
                {
                    UserId = user.id,
                    Email = user.email
                }
            }
        },
        config => config
            .Including(x => x.InviteToken)
            .Including(x => x.Organisation.Id)
            .Including(x => x.Organisation.Name)
            .Including(x => x.Person.Email)
            .Including(x => x.Person.FirstName)
            .Including(x => x.Person.LastName)
            .Including(x => x.Person.Telephone)
            .Including(x => x.User.UserId)
            .Including(x => x.Person.User.Email)
        );
    }

    private static (Guid id, string email) GetTestUser()
    {
        return (Guid.NewGuid(), $"{Guid.NewGuid()}@example.com");
    }

    private static AccountsDbContext CreateAssertContext()
    {
        return new AccountsDbContext(
            new DbContextOptionsBuilder<AccountsDbContext>()
                .UseSqlServer(_database.ConnectionString)
                .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                .EnableSensitiveDataLogging()
                .Options);
    }

    private static async Task<Person> AddPerson(Guid userId, string email)
    {
        var person = new Person
        {
            FirstName = "FirstName",
            LastName = "LastName",
            Email = email,
            Telephone = "01234567890",
            User = new User
            {
                UserId = userId,
                Email = email
            }
        };

        _context.Persons.Add(person);

        await _context.SaveChangesAsync("service id");

        return person;
    }

    private static int _testOrganisationOrdinal;

    private static ReprocessorExporterOrganisationModel GetTestReprocessorExporterOrganisationModel()
    {
        int num = Interlocked.Increment(ref _testOrganisationOrdinal);

        return new ReprocessorExporterOrganisationModel
        {
            Address = new AddressModel
            {
                SubBuildingName = $"SubBuildingName({num})",
                BuildingName = $"BuildingName({num})",
                BuildingNumber = $"BuildingNumber({num})",
                Street = $"Street({num})",
                Locality = $"Locality({num})",
                DependentLocality = $"DependentLocality({num})",
                County = $"County({num})",
                Country = $"Country({num})",
                Postcode = $"PC({num})",
                Town = $"Town({num})"
            },
            CompaniesHouseNumber = $"CH({num})",
            Name = $"Name({num})",
            TradingName = $"TradingName({num})",
            OrganisationType = Core.Models.OrganisationType.CompaniesHouseCompany,
            Nation = Nation.England,
            IsComplianceScheme = false
        };
    }
}