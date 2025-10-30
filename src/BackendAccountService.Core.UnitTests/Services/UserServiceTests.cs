using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;
using Service = BackendAccountService.Data.Entities.Service;
using ServiceRole = BackendAccountService.Data.Entities.ServiceRole;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class UserServiceTests
{
    private AccountsDbContext _dbContext;
    private UserService _userService;
    private readonly NullLogger<UserService> _nullLogger = new();
    private Mock<IValidationService> _validationService;
    private const string User1Email = "user1@test.com";
    private const string User5Email = "user5@test.com";
    private const string User6Email = "user6@test.com";
    private const string User5InviteToken = "User5InviteToken";
    private const string User7Email = "system@dummy.com";
    private const string User7UserId = "0433FE2C-1884-491A-B3A2-67A7DC2B9840";
    private const string User8UserId = "d272a9b9-79ab-4159-9ff1-cf0f9c903f69";
    private const string OrganisationId4 = "4939f8eb-b6fd-4f26-87e3-0b9d2948bd90";
    private static readonly Guid User9UserId = Guid.NewGuid();
    private static readonly Guid User10UserId = Guid.NewGuid();
    private static readonly Guid User11UserId = Guid.NewGuid();
    private const string User9Email = "user9@check.com";
    private const string User10Email = "user10@check.com";
    private const string User11Email = "user11@check.com";
    private static readonly Guid OrganisationId5 = Guid.NewGuid();
    private static readonly Guid OrganisationId6 = Guid.NewGuid();
    private static readonly Guid OrganisationId7 = Guid.NewGuid();
    private static readonly Guid OrganisationId8 = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _dbContext = new AccountsDbContext(contextOptions);
        _validationService = new Mock<IValidationService>();

        _userService = new UserService(_dbContext, _nullLogger, _validationService.Object);
    }

    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

        var organisation1 = new Organisation
        {
            Name = "Org 1",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "111111",
            ExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd87")
        };

        var organisation2 = new Organisation
        {
            Name = "Org 2",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "222222",
            ExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd88"),
        };

        var organisation3 = new Organisation
        {
            Name = "Org 3",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "333333",
            ExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd89")
        };

        var organisation4 = new Organisation
        {
            Name = "Org 4",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "444444",
            ExternalId = new Guid(OrganisationId4)
        };

        var organisation5 = new Organisation
        {
            Name = "Org 5",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "555555",
            ExternalId = OrganisationId5
        };

        var organisation6 = new Organisation
        {
            Name = "Org 6",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "666666",
            ExternalId = OrganisationId6,
            IsComplianceScheme = true
        };

        var organisation7 = new Organisation
        {
            Name = "Org 7",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "777777",
            ExternalId = OrganisationId7
        };

        var organisation8 = new Organisation
        {
            Name = "Org 8",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "888888",
            ExternalId = OrganisationId8
        };

        setupContext.Organisations.Add(organisation1);
        setupContext.Organisations.Add(organisation2);
        setupContext.Organisations.Add(organisation3);
        setupContext.Organisations.Add(organisation4);
        setupContext.Organisations.Add(organisation5);
        setupContext.Organisations.Add(organisation6);
        setupContext.Organisations.Add(organisation7);
        setupContext.Organisations.Add(organisation8);

        var OrgConnection = new OrganisationsConnection
        {
            FromOrganisationId = 1,
            FromOrganisationRoleId = 1,
            ToOrganisationId = 2,
            ToOrganisationRoleId = 2
        };
        setupContext.OrganisationsConnections.Add(OrgConnection);

        var complianceScheme = new ComplianceScheme
        {
            Name = "Compliance Scheme 1",
            CompaniesHouseNumber = organisation1.CompaniesHouseNumber
        };
        setupContext.ComplianceSchemes.Add(complianceScheme);

        var selectedScheme = new SelectedScheme
        {
            ComplianceSchemeId = 1,
            OrganisationConnectionId = 1
        };
        setupContext.SelectedSchemes.Add(selectedScheme);

        var user = new User
        {
            UserId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2"),
            Email = User1Email
        };
        var user2 = new User
        {
            UserId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca3"),
            Email = "user2@test.com"
        };

        var user3 = new User
        {
            UserId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca4"),
            ExternalIdpUserId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca5").ToString(),
            Email = "user3@test.com"
        };
        var user4 = new User
        {
            UserId = new Guid("d272a9b9-79ab-4159-9ff1-cf0f9c903f66"),
            Email = "user4@test.com",
            InviteToken = "NTHAPJPaUhX2BjFxU0LT8T2UyXjwTAeNs895lqPZd6NRhrwaWvgBp4U-zk1ngj3WPi384zE9j-BStDs4lzFG9g=="
        };

        var user5 = new User
        {
            UserId = new Guid("d272a9b9-79ab-4159-9ff1-cf0f9c903f67"),
            Email = User5Email,
            InviteToken = User5InviteToken
        };

        var user6 = new User
        {
            UserId = new Guid("d272a9b9-79ab-4159-9ff1-cf0f9c903f68"),
            Email = User6Email,
        };

        var user7 = new User
        {
            UserId = new Guid(User7UserId),
            Email = User7Email,
        };

        var user8 = new User
        {
            UserId = new Guid(User8UserId),
            Email = "user8@test.com",
            IsDeleted = true
        };

        var user9 = new User
        {
            UserId = User9UserId,
            Email = User9Email,
        };

        var user10 = new User
        {
            UserId = User10UserId,
            Email = User10Email,
        };

        var user11 = new User
        {
            UserId = User11UserId,
            Email = User11Email,
        };

        setupContext.Users.Add(user);
        setupContext.Users.Add(user2);
        setupContext.Users.Add(user3);
        setupContext.Users.Add(user4);
        setupContext.Users.Add(user5);
        setupContext.Users.Add(user6);
        setupContext.Users.Add(user7);
        setupContext.Users.Add(user8);
        setupContext.Users.Add(user9);
        setupContext.Users.Add(user10);
        setupContext.Users.Add(user11);

        var person = new Person
        {
            UserId = 1,
            ExternalId = new Guid("b871997f-c3d1-4906-8094-f3cd131c88d3"),
            FirstName = "Test",
            LastName = "User",
            Email = "test@abc.com",
            Telephone = "0123456789",
            User = user
        };

        var person2 = new Person
        {
            UserId = 2,
            FirstName = "Test",
            LastName = "User2",
            Email = "test2@abc.com",
            Telephone = "0123456789",
        };

        var person3 = new Person
        {
            UserId = 3,
            FirstName = "Test",
            LastName = "User3",
            Email = "test3@abc.com",
            Telephone = "0123456789"
        };

        var person4 = new Person
        {
            UserId = 4,
            FirstName = "Test4",
            LastName = "User4",
            Email = "test4@abc.com",
            Telephone = "0123456444",
            User = user6
        };

        var person5 = new Person
        {
            UserId = 5,
            FirstName = "Test5",
            LastName = "User7",
            Email = "test5@abc.com",
            Telephone = "0123456555",
            User = user7
        };

        var person6 = new Person
        {
            UserId = 6,
            FirstName = "Test5",
            LastName = "User7",
            Email = "test5@abc.com",
            Telephone = "0123456555",
            User = user8
        };

        var person7 = new Person
        {
            FirstName = "Test",
            LastName = "person7",
            Email = "testperson7@abc.com",
            Telephone = "0123456555",
            User = user9
        };

        var person8 = new Person
        {
            FirstName = "Test",
            LastName = "person8",
            Email = "testperson8@abc.com",
            Telephone = "0123456555",
            User = user10
        };

        var person9 = new Person
        {
            FirstName = "Test",
            LastName = "person9",
            Email = "testperson9@abc.com",
            Telephone = "0123456555",
            User = user11
        };

        setupContext.Persons.Add(person);
        setupContext.Persons.Add(person2);
        setupContext.Persons.Add(person3);
        setupContext.Persons.Add(person4);
        setupContext.Persons.Add(person5);
        setupContext.Persons.Add(person6);
        setupContext.Persons.Add(person7);
        setupContext.Persons.Add(person8);
        setupContext.Persons.Add(person9);

        user.Person = person;
        user6.Person = person4;

        var personOrganisationConnection = new PersonOrganisationConnection
        {
            PersonId = 1,
            OrganisationId = 1,
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation1,
            Person = person
        };

        var personOrganisationConnection2 = new PersonOrganisationConnection
        {
            PersonId = 2,
            OrganisationId = 2,
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation2
        };

        var personOrganisationConnection3 = new PersonOrganisationConnection
        {
            PersonId = 3,
            OrganisationId = 1,
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation3,
            Person = person3
        };

        person3.OrganisationConnections = new List<PersonOrganisationConnection>
        {
            personOrganisationConnection3
        };

        var personOrganisationConnection4 = new PersonOrganisationConnection
        {
            PersonId = 4,
            OrganisationId = 4,
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation4,
            Person = person4
        };

        var personOrganisationConnection5 = new PersonOrganisationConnection
        {
            PersonId = 5,
            OrganisationId = 4,
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation4,
            Person = person5
        };

        var personOrganisationConnection6 = new PersonOrganisationConnection
        {
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation5,
            Person = person7
        };

        var personOrganisationConnection7 = new PersonOrganisationConnection
        {
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation6,
            Person = person7
        };

        var personOrganisationConnection8 = new PersonOrganisationConnection
        {
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation7,
            Person = person7
        };


        var personOrganisationConnection9 = new PersonOrganisationConnection
        {
            OrganisationRoleId = 1,
            PersonRoleId = 2,
            Organisation = organisation8,
            Person = person8
        };

        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection2);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection3);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection4);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection5);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection6);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection7);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection8);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection9);

        var packagingService = new Service
        {
            Key = ServiceKeys.Packaging,
            Name = "EPR Packaging"
        };

        var reExService = new Service
        {
            Key = ServiceKeys.ReprocessorExporter,
            Name = "EPR for packaging: reprocessors and exporters"
        };

        setupContext.Services.Add(packagingService);
        setupContext.Services.Add(reExService);

        var packagingApprovedPersonRole = new ServiceRole
        {
            Service = packagingService,
            Key = "Packaging.ApprovedPerson",
            Name = "Approved Person"
        };

        var reExApprovedPersonRole = new ServiceRole
        {
            Service = reExService,
            Key = "Re-Ex.ApprovedPerson",
            Name = "Approved Person"
        };

        setupContext.ServiceRoles.Add(packagingApprovedPersonRole);
        setupContext.ServiceRoles.Add(reExApprovedPersonRole);

        var enrolment = new Enrolment
        {
            ServiceRoleId = 3,
            EnrolmentStatusId = 3,
            Connection = personOrganisationConnection2
        };

        var enrolment1 = new Enrolment
        {
            ServiceRoleId = 3,
            EnrolmentStatusId = 3,
            Connection = personOrganisationConnection3
        };

        var enrolment2 = new Enrolment
        {
            ServiceRole = packagingApprovedPersonRole,
            EnrolmentStatusId = 3,
            Connection = personOrganisationConnection6
        };

        var enrolment3 = new Enrolment
        {
            ServiceRole = packagingApprovedPersonRole,
            EnrolmentStatusId = 3,
            Connection = personOrganisationConnection7
        };

        var enrolment4 = new Enrolment
        {
            ServiceRole = reExApprovedPersonRole,
            EnrolmentStatusId = 3,
            Connection = personOrganisationConnection8
        };

        var enrolment5 = new Enrolment
        {
            ServiceRole = reExApprovedPersonRole,
            EnrolmentStatusId = 3,
            Connection = personOrganisationConnection9
        };

        setupContext.Enrolments.Add(enrolment);
        setupContext.Enrolments.Add(enrolment1);
        setupContext.Enrolments.Add(enrolment2);
        setupContext.Enrolments.Add(enrolment3);
        setupContext.Enrolments.Add(enrolment4);
        setupContext.Enrolments.Add(enrolment5);



        var changeHistory = new ChangeHistory
        {
            Person = person,
            Organisation = organisation1,
            IsActive = true,
            DecisionDate = null,
            IsDeleted = false
        };

        var changeHistory1 = new ChangeHistory
        {
            Person = person6,
            Organisation = organisation5,
            IsActive = true,
            DecisionDate = null,
            IsDeleted = false
        };

        setupContext.ChangeHistory.Add(changeHistory);
        setupContext.ChangeHistory.Add(changeHistory1);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    [TestMethod]
    public async Task GetUserOrganisationAsync_ReturnsSoftDeletedUserInformation()
    {
        //Setup
        //Act
        var result = await _userService.GetUserOrganisationAsync(new Guid(User8UserId!), true);

        //Assert

        result.Should().BeOfType(typeof(Result<UserOrganisationsListModel>));
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenUserListOfOrganisationsIsRequested_ThenReturnListOfUserOrganisations()
    {
        //Setup
        var userId = "5dc5267b-ed00-4551-9129-4abc9944aca4";

        //Act
        var result = await _userService.GetUserOrganisationAsync(new Guid(userId!));

        //Assert

        result.Should().BeOfType(typeof(Result<UserOrganisationsListModel>));
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenNoOrganisationIsFound_ThenReturnNotFound()
    {
        //Setup
        var userId = Guid.NewGuid().ToString();

        //Act
        var expectedResult = Result<UserOrganisationsListModel>.FailedResult($"No user found with the user id {userId}", HttpStatusCode.NotFound);

        //Assert
        var result = await _userService.GetUserOrganisationAsync(new Guid(userId));

        result.Should().BeOfType(typeof(Result<UserOrganisationsListModel>));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task WhenUserListOfOrganisationIdsIsRequested_ThenReturnListOfUserOrganisationIds()
    {
        //Setup
        var userId = "5dc5267b-ed00-4551-9129-4abc9944aca4";

        //Act
        var result = await _userService.GetUserOrganisationIdListAsync(new Guid(userId!));

        //Assert
        result.Should().BeOfType(typeof(Result<IList<string>>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public async Task WhenNoOrganisationIsFound_Then_GetUserOrganisationIdListAsync_ReturnsEmptyIdList()
    {
        //Setup
        var userId = Guid.NewGuid().ToString();

        //Act
        var result = await _userService.GetUserOrganisationIdListAsync(new Guid(userId));

        //Assert
        result.Should().BeOfType(typeof(Result<IList<string>>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetPersonOrganisation_WhenServiceKeyNull_ThenReturnNull()
    {
        var result = await _userService.GetPersonOrganisationsWithEnrolmentsForServiceAsync(It.IsAny<Guid>(), string.Empty);

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetPersonOrganisation_WhenUserDoesNotExist_ThenReturnNotFound()
    {
        //Setup
        var userId = new Guid("1dc5267b-ed00-4551-9129-4abc9944aca1");
        var serviceKey = "serviceKey";

        //Act
        var expectedResult = Result<PersonWithOrganisationsResponse>.FailedResult($"No user details found for user {userId}", HttpStatusCode.NotFound);

        //Assert
        var result = await _userService.GetPersonOrganisationsWithEnrolmentsForServiceAsync(userId, serviceKey);

        result.Should().BeOfType(typeof(Result<PersonWithOrganisationsResponse>));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task GetPersonOrganisation_WhenUserExists_ThenReturnListOfPersonOrganisations()
    {
        //Setup
        var expectedUser = await _dbContext.Users.FirstOrDefaultAsync(user => !string.IsNullOrWhiteSpace(user.ExternalIdpUserId));
        var userId = expectedUser.ExternalIdpUserId;
        var expectedPerson = await _dbContext.Persons.FirstOrDefaultAsync(person => person.UserId == expectedUser.Id);
        var serviceKey = "serviceKey";

        //Act
        var result = await _userService.GetPersonOrganisationsWithEnrolmentsForServiceAsync(new Guid(userId), serviceKey);

        //Assert

        result.Should().BeOfType(typeof(Result<PersonWithOrganisationsResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(expectedPerson.FirstName);
        result.Value.LastName.Should().Be(expectedPerson.LastName);
        result.Value.Email.Should().Be(expectedPerson.Email);
        result.Value.Organisations.Count.Should().Be(1);
    }

    [TestMethod]
    public async Task GetApprovedUserUserByEmail_returns_expected_user()
    {
        //Setup

        //Act
        var result = await _userService.GetApprovedUserUserByEmailAsync(User1Email);

        //Assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetUserByInviteAsync_returns_expected_user()
    {
        //Setup

        //Act
        var result = await _userService.GetUserByInviteAsync(User5Email, User5InviteToken);

        //Assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    [DataRow(new string[] { }, new string[] { })]
    [DataRow(new[] { User5Email }, new[] { User5Email })]
    [DataRow(new[] { User5Email, User6Email }, new[] { User5Email, User6Email })]
    [DataRow(new[] { "snollygoster@example.com", "cattywampus@example.com", "bumfuddle@example.com" }, new string[] { })]
    [DataRow(new[] { User5Email, "bumfuddle@example.com" }, new[] { User5Email })]
    public async Task DoAnyUsersExist_ReturnsExistingUserEmail(string[] usersToCheck, string[] expectedResult)
    {
        // Act 
        var result = await _userService.DoAnyUsersExist(usersToCheck);

        // Arrange
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task DoAnyUsersExist_UserDoesNotExists_Returns()
    {
        // Act 
        var result = await _userService.DoAnyUsersExist([User5Email]);

        // Arrange
        result.Should().BeEquivalentTo(User5Email);
    }

    [TestMethod]
    public async Task InvitationTokenExists_ReturnsTrue_WhenTokenExists()
    {
        // Act
        var result = await _userService.InvitationTokenExists("NTHAPJPaUhX2BjFxU0LT8T2UyXjwTAeNs895lqPZd6NRhrwaWvgBp4U-zk1ngj3WPi384zE9j-BStDs4lzFG9g==");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task InvitationTokenExists_ReturnsFalse_WhenTokenDoesNotExist()
    {
        // Act
        var result = await _userService.InvitationTokenExists("invalidToken");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Passing_InvalidUserId_Returns_Notfound()
    {
        //Arrange
        var userId = Guid.Empty;
        var request = new Models.Request.UpdateUserDetailsRequest();

        //Act
        var result = await _userService.UpdateUserDetailsRequest(userId, Guid.NewGuid(), "serviceKey", request);

        //Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Passing_InvalidUserId_Returns_Forbidden()
    {
        //Arrange   
        var userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");
        var organisationExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd87");
        var request = new Models.Request.UpdateUserDetailsRequest();

        //Act   
        var result = await _userService.UpdateUserDetailsRequest(userId, organisationExternalId, "serviceKey", request);

        //Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Returns_NotFound_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;
        var organisationExternalId = Guid.NewGuid();
        var request = new Models.Request.UpdateUserDetailsRequest();

        // Act
        var result = await _userService.UpdateUserDetailsRequest(userId, organisationExternalId, "serviceKey", request);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Returns_NotFound_WhenOrganisationNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationExternalId = Guid.NewGuid();
        var request = new Models.Request.UpdateUserDetailsRequest();

        // Act
        var result = await _userService.UpdateUserDetailsRequest(userId, organisationExternalId, "serviceKey", request);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Updates_Telephone_WhenBasicUserInEprPackaging()
    {
        // Arrange
        var userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");
        var organisationExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd87");
        var request = new Models.Request.UpdateUserDetailsRequest
        {
            FirstName = "Test",
            LastName = "User",
            Telephone = "0123456780"
        };

        _validationService.Setup(v => v.IsBasicUserInEprPackaging(userId, organisationExternalId, "serviceKey"))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserDetailsRequest(userId, organisationExternalId, "serviceKey", request);

        // Assert
        result.Should().NotBeNull();
        result.Value.HasTelephoneOnlyUpdated.Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Updates_BasicUserDetails()
    {
        // Arrange
        var userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");
        var organisationExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd87");
        var request = new Models.Request.UpdateUserDetailsRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Telephone = "0123456789",
            JobTitle = "Developer"
        };

        _validationService.Setup(v => v.IsBasicUserInEprPackaging(userId, organisationExternalId, "serviceKey"))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserDetailsRequest(userId, organisationExternalId, "serviceKey", request);

        // Assert
        result.Should().NotBeNull();
        result.Value.HasBasicUserDetailsUpdated.Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Creates_ChangeHistory_ForApproval()
    {
        // Arrange
        var userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca3");
        var organisationExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd88");
        var request = new Models.Request.UpdateUserDetailsRequest
        {
            FirstName = "John",
            LastName = "Doe",
            JobTitle = "Manager"
        };

        _validationService.Setup(v => v.IsApprovedOrDelegatedUserInEprPackaging(userId, organisationExternalId, "serviceKey"))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserDetailsRequest(userId, organisationExternalId, "serviceKey", request);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.HasApprovedOrDelegatedUserDetailsSentForApproval.Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Returns_BadRequest_WhenChangeHistoryExists()
    {
        // Arrange
        var userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");
        var organisationExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd87");
        var request = new Models.Request.UpdateUserDetailsRequest
        {

            FirstName = "John",
            LastName = "Doe",
            JobTitle = "Manager"
        };

        _validationService.Setup(v => v.IsApprovedOrDelegatedUserInEprPackaging(userId, organisationExternalId, "serviceKey"))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserDetailsRequest(userId, organisationExternalId, "serviceKey", request);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task UpdateUserDetailsRequest_Returns_InternalServerError_OnException()
    {
        // Arrange
        var userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");
        var organisationExternalId = new Guid("4939f8eb-b6fd-4f26-87e3-0b9d2948bd87");
        var request = new Models.Request.UpdateUserDetailsRequest();

        _validationService.Setup(x =>
                x.IsApprovedOrDelegatedUserInEprPackaging(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _userService.UpdateUserDetailsRequest(userId, organisationExternalId, "serviceKey", request);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetUserByUserId_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");

        // Act
        var result = await _userService.GetUserByUserId(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.UserId);

        Assert.IsNotNull(result.Person);
        Assert.AreEqual("Test", result.Person.FirstName);
        Assert.AreEqual("User", result.Person.LastName);
        Assert.AreEqual("user1@test.com", result.Email);
    }

    [TestMethod]
    public async Task GetUserByUserId_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _userService.GetUserByUserId(userId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetSystemUserAndOrganisationAsync_ReturnSystemUserAndOrganisation_WhenSystemUserExist()
    {
        // Arrange

        // Act
        var result = await _userService.GetSystemUserAndOrganisationAsync(ApplicationUser.System);

        // Assert
        result.Should().BeOfType(typeof(Result<UserOrganisation>));
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(User7UserId);
        result.Value.OrganisationId.Should().Be(OrganisationId4);
    }

    [TestMethod]
    public async Task GetSystemUserAndOrganisationAsync_ReturnNotFound_WhenSystemUserDoesNotExist()
    {
        // Arrange
        var invalidEmail = "Invalid.System.Email@dummy.com";
        var expectedResult = Result<UserOrganisation>.FailedResult($"Error fetching system user for {invalidEmail}", HttpStatusCode.NotFound);

        // Act
        var result = await _userService.GetSystemUserAndOrganisationAsync(invalidEmail);

        // Assert
        result.Should().BeOfType(typeof(Result<UserOrganisation>));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task GetUserOrganisationsWithEnrolmentsAsync_WithEmptyServiceKey_ReturnsAllEnrolments()
    {
        // Arrange
        var serviceKey = string.Empty;

        // Act
        var result = await _userService.GetUserOrganisationsWithEnrolmentsAsync(User9UserId, serviceKey);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.User.Organisations.SelectMany(o => o.Enrolments).Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task GetUserOrganisationsWithEnrolmentsAsync_WhenUserNotFound_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var serviceKey = "any-service";

        // Act
        var result = await _userService.GetUserOrganisationsWithEnrolmentsAsync(userId, serviceKey);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.ErrorMessage.Should().Be("User not found.");
    }

    [TestMethod]
    public async Task GetUserOrganisationsWithEnrolmentsAsync_WhenUserFound_ReturnsPackagingOrganisation()
    {
        // Arrange
        var serviceKey = ServiceKeys.Packaging;

        // Act
        var result = await _userService.GetUserOrganisationsWithEnrolmentsAsync(User9UserId, serviceKey);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.User.NumberOfOrganisations.Should().Be(2);
        result.Value.User.Organisations.Count.Should().Be(2);
        result.Value.User.Organisations.Exists(o => o.OrganisationRole == OrganisationRoles.ComplianceScheme);
        result.Value.User.Organisations.Exists(o => o.OrganisationRole == OrganisationRoles.Producer);
        result.Value.User.Organisations.SelectMany(o => o.Enrolments).Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task GetUserOrganisationsWithEnrolmentsAsync_WhenUserFound_ReturnsReExOrganisation()
    {
        // Arrange
        var serviceKey = ServiceKeys.ReprocessorExporter;

        // Act
        var result = await _userService.GetUserOrganisationsWithEnrolmentsAsync(User9UserId, serviceKey);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.User.NumberOfOrganisations.Should().Be(1);
        result.Value.User.Organisations.Count.Should().Be(1);
        result.Value.User.Organisations.Exists(o => o.OrganisationRole == OrganisationRoles.ReprocessorExporter);
        result.Value.User.Organisations.SelectMany(o => o.Enrolments).Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task GetUserOrganisationsWithEnrolmentsAsync_ReturnsUserModelWithOutOrganisation_WithUserDataDefaultToFirstOrganisationEnroment()
    {
        // Arrange
        var serviceKey = string.Empty;

        // Act
        var result = await _userService.GetUserOrganisationsWithEnrolmentsAsync(User10UserId, serviceKey);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.User.NumberOfOrganisations.Should().Be(1);
        result.Value.User.Organisations.Count.Should().Be(1);
        result.Value.User.Organisations.SelectMany(o => o.Enrolments).Should().NotBeEmpty();
        result.Value.User.ServiceRoleId.Should().NotBe(0);
        result.Value.User.ServiceRole.Should().NotBeNullOrEmpty();
        result.Value.User.Service.Should().NotBeNullOrEmpty();
        result.Value.User.EnrolmentStatus.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public async Task GetUserOrganisationsWithEnrolmentsAsync_ReturnsUserModelWithOutOrganisation_WhenUserHasNoConnetion()
    {
        // Arrange
        var serviceKey = string.Empty;

        // Act
        var result = await _userService.GetUserOrganisationsWithEnrolmentsAsync(User11UserId, serviceKey);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.User.NumberOfOrganisations.Should().Be(0);
        result.Value.User.Organisations.Count.Should().Be(0);
        result.Value.User.JobTitle.Should().BeNullOrEmpty();
        result.Value.User.RoleInOrganisation.Should().BeNullOrEmpty();
        result.Value.User.IsChangeRequestPending.Should().Be(false);
        result.Value.User.ServiceRoleId.Should().Be(0);
        result.Value.User.ServiceRole.Should().BeNullOrEmpty();
        result.Value.User.Service.Should().BeNullOrEmpty();
        result.Value.User.EnrolmentStatus.Should().BeNullOrEmpty();
    }

    [TestMethod]
    public async Task GetUserIdByPersonId_ShouldReturnUserId_WhenUserExists()
    {
        // Arrange
        var personId = new Guid("b871997f-c3d1-4906-8094-f3cd131c88d3");
        var expectedUserId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");

        // Act
        var result = await _userService.GetUserIdByPersonId(personId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedUserId, result);
    }

    [TestMethod]
    public async Task GetUserIdByPersonId_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var personId = Guid.NewGuid();

        // Act
        var result = await _userService.GetUserIdByPersonId(personId);

        // Assert
        Assert.IsNull(result);
    }
}