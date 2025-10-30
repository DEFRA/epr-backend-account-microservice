using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Service = BackendAccountService.Data.Entities.Service;
using ServiceRole = BackendAccountService.Data.Entities.ServiceRole;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class NotificationsServiceTests
{
    private AccountsDbContext _accountContext= null!;
    private NotificationsService _notificationsService = null!;
    private static DbContextOptions<AccountsDbContext> _contextOptions = default;
    
    private readonly string validServiceKey = "Packaging";
    private readonly string invalidServiceKey = "DummyService";
    private static readonly Guid validNominatedUserId = new("00000000-0000-0000-0000-000000000010");
    private static readonly Guid validPendingUserId = new("00000000-0000-0000-0000-000000000020");
    private static readonly Guid validApprovedUserId = new("00000000-0000-0000-0000-000000000030");
    private static readonly Guid validOrganisationId = new("00000000-0000-0000-0000-000000000040");

    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        _contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("NotificationsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options; 

        SetUpDatabase(_contextOptions);
    }

    [TestInitialize]
    public void Setup()
    {
        _accountContext = new AccountsDbContext(_contextOptions);
        _notificationsService = new NotificationsService(_accountContext);
    }

    [TestMethod]
    public async Task GetNotificationsForServiceAsync_WhenValidRequestAndDelegatedPersonNominationNotificationExists_ThenReturnNotifications()
    {
        var result = await _notificationsService.GetNotificationsForServiceAsync(validNominatedUserId, validOrganisationId, validServiceKey);
        result.Should().BeOfType<NotificationsResponse>();
        result.Notifications.Should().HaveCount(1);
        result.Notifications[0].Type.Should().Be(NotificationTypes.Packaging.DelegatedPersonNomination);
        result.Notifications[0].Data.ToList()[0].Key.Should().Be("EnrolmentId");
    }

    [TestMethod]
    public async Task GetNotificationsForServiceAsync_WhenValidRequestAndDelegatedPersonPendingApprovalNotificationExists_ThenReturnNotifications()
    {
        var result = await _notificationsService.GetNotificationsForServiceAsync(validPendingUserId, validOrganisationId, validServiceKey);
        result.Should().BeOfType<NotificationsResponse>();
        result.Notifications.Should().HaveCount(1);
        result.Notifications[0].Type.Should().Be(NotificationTypes.Packaging.DelegatedPersonPendingApproval);
        result.Notifications[0].Data.ToList()[0].Key.Should().Be("EnrolmentId");
    }

    [TestMethod]
    public async Task GetNotificationsForServiceAsync_WhenValidRequestAndNoNotificationsExist_ThenReturnEmptyList()
    {
        var result = await _notificationsService.GetNotificationsForServiceAsync(validApprovedUserId, validOrganisationId, validServiceKey);
        result.Should().BeOfType<NotificationsResponse>();
        result.Notifications.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task GetNotificationsForServiceAsync_WhenNonPackagingService_ThenReturnEmptyList()
    {
        var result = await _notificationsService.GetNotificationsForServiceAsync(validPendingUserId, validOrganisationId, invalidServiceKey);
        result.Should().BeOfType<NotificationsResponse>();
        result.Notifications.Should().HaveCount(0);
    }


    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        setupContext.Database.EnsureCreated();

        var organisation = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "123456",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 1",
            SubBuildingName = "Sub building 1",
            BuildingName = "Building 1",
            BuildingNumber = "1",
            Street = "Street 1",
            Locality = "Locality 1",
            DependentLocality = "Dependent Locality 1",
            Town = "Town 1",
            County = "County 1",
            Postcode = "BT44 5QW",
            Country = "Country 1",
            NationId = Data.DbConstants.Nation.England,
            ExternalId = validOrganisationId
        };

        setupContext.Organisations.Add(organisation);
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);

        var nominatedEnrolment = new Enrolment
        {
            ExternalId = Guid.NewGuid(),
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Id,
            ServiceRole = new ServiceRole()
            {
                //Id = Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Id,
                Key = Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Key,
                Name = "Delegated Person",
                Service = new Service() 
                { 
                    Key = "Packaging",
                    Name = "EPR Packaging"
                }

            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Nominated,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = Guid.NewGuid(),
                JobTitle = "Director",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = "Joey",
                    LastName = "Ramone",
                    Email = "joey@ramones.com",
                    Telephone = "07012341234",
                    User = new User
                    {
                        UserId = validNominatedUserId,
                        Email = "joey@ramones.com"
                    }
                },
                OrganisationId = organisation.Id
            }
        };

        setupContext.Enrolments.Add(nominatedEnrolment);

        var pendingEnrolment = new Enrolment
        {
            ExternalId = Guid.NewGuid(),
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Id,
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Pending,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = Guid.NewGuid(),
                JobTitle = "Chief Financial Officer",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = "Johnny",
                    LastName = "Ramone",
                    Email = "johnny@ramones.com",
                    Telephone = "07012341234",
                    User = new User
                    {
                        UserId = validPendingUserId,
                        Email = "johnny@ramones.com"
                    }
                },
                OrganisationId = organisation.Id
            }
        };

        setupContext.Enrolments.Add(pendingEnrolment);

        var activeEnrolment = new Enrolment
        {
            ExternalId = Guid.NewGuid(),
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Id,
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Approved,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = Guid.NewGuid(),
                JobTitle = "Chief Executive Officer",
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin,
                Person = new Person
                {
                    FirstName = "Tommy",
                    LastName = "Ramone",
                    Email = "tommy@ramones.com",
                    Telephone = "07012341234",
                    User = new User
                    {
                        UserId = validApprovedUserId,
                        Email = "tommy@ramones.com"
                    }
                },
                OrganisationId = organisation.Id
            }
        };

        setupContext.Enrolments.Add(activeEnrolment);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}
