using System.Net;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class UserServiceTests
{
    private AccountsDbContext _dbContext;
    private UserService _userService;
    private readonly NullLogger<UserService> _nullLogger = new();
    private const string User1Email = "user1@test.com";

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _dbContext = new AccountsDbContext(contextOptions);
        _userService = new UserService(_dbContext, _nullLogger);
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
           CompaniesHouseNumber = "111111"
        };

        var organisation2 = new Organisation
        {
            Name = "Org 2",
            OrganisationTypeId = 1,
            CompaniesHouseNumber = "222222"
        };
        setupContext.Organisations.Add(organisation1);
        setupContext.Organisations.Add(organisation2);

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
            UserId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca3"),
            ExternalIdpUserId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca4").ToString(),
            Email = "user3@test.com"
        };

        setupContext.Users.Add(user);
        setupContext.Users.Add(user2);
        setupContext.Users.Add(user3);

        var person = new Person
        {
            UserId = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@abc.com",
            Telephone = "0123456789"
        };

        var person2 = new Person
        {
            UserId = 2,
            FirstName = "Test",
            LastName = "User2",
            Email = "test2@abc.com",
            Telephone = "0123456789"
        };

        var person3 = new Person
        {
            UserId = 3,
            FirstName = "Test",
            LastName = "User3",
            Email = "test3@abc.com",
            Telephone = "0123456789"
        };
        setupContext.Persons.Add(person);
        setupContext.Persons.Add(person2);
        setupContext.Persons.Add(person3);

        var personOrganisationConnection = new PersonOrganisationConnection
        {
            PersonId = 1,
            OrganisationId = 1,
            OrganisationRoleId = 1,
            PersonRoleId = 2
        };

        var personOrganisationConnection2 = new PersonOrganisationConnection
        {
            PersonId = 3,
            OrganisationId = 1,
            OrganisationRoleId = 1,
            PersonRoleId = 2,
        };
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection);
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection2);

        var enrolment = new Enrolment
        {
            ServiceRoleId = 3,
            EnrolmentStatusId = 3,
            Connection = personOrganisationConnection2
        };
        setupContext.Enrolments.Add(enrolment);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    [TestMethod]
    public async Task WhenUserListOfOrganisationsIsRequested_ThenReturnListOfUserOrganisations()
    {
        //Setup
        var expectedUser = _dbContext.Users.First();
        var userId = expectedUser.UserId.ToString();

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
        var userId = "5dc5267b-ed00-4551-9129-4abc9944aca4";

        //Act
        var expectedResult = Result<UserOrganisationsListModel>.FailedResult($"No user found with the user id {userId}", HttpStatusCode.NotFound);

        //Assert
        var result = await _userService.GetUserOrganisationAsync(new Guid(userId));

        result.Should().BeOfType(typeof(Result<UserOrganisationsListModel>));
        result.Should().BeEquivalentTo(expectedResult);
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
        var expectedUser = _dbContext.Users.FirstOrDefault(user => !user.ExternalIdpUserId.IsNullOrEmpty());
        var userId = expectedUser.ExternalIdpUserId;
        var expectedPerson = _dbContext.Persons.FirstOrDefault(person => person.UserId == expectedUser.Id);
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
}