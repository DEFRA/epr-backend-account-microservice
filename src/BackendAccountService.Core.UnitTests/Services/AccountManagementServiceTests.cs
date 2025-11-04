using System.ComponentModel.DataAnnotations;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using BackendAccountService.Core.UnitTests.TestHelpers;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using BackendAccountService.Data.DbConstants;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class AccountManagementServiceTests
{
    private const int UserIdToEnroll = AccountManagementTestHelper.UserIdToEnroll;
    
    private AccountsDbContext _accountContext = null!;
    private AccountManagementService _sut = null!;
    private readonly NullLogger<AccountManagementService> _logger = new();
    private TokenService _tokenService = null!;
    private EnrolmentsService _enrolmentsService = null!;
    private readonly Guid _existingOrgId = new Guid("00000000-0000-0000-0000-000000000001");
    
    private DbContextOptions<AccountsDbContext> _dbContextOptions = null!;

    [TestInitialize]
    public void Setup()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _accountContext = new AccountsDbContext(_dbContextOptions);
        AccountManagementTestHelper.SetupDatabaseForInviteUser(_accountContext);
       
        _enrolmentsService = new EnrolmentsService(_accountContext, new NullLogger<EnrolmentsService>());

        _tokenService = new TokenService();
        _sut = new AccountManagementService(_tokenService, _logger, _accountContext);
    }

    [TestMethod]
    public async Task When_User_Is_Invited_With_Valid_Data_Then_Add_User_Record()
    {
        //Arrange
        var request = CreateInviteUserRequest();
      
        //Act
        var result = await _sut.CreateInviteeAccountAsync(request);
        var isUserAdded = _accountContext.Users.Any(user => user.Email == request.InvitedUser.Email && !user.IsDeleted);
       
        //Assert
        result.Should().BeOfType(typeof(string));
        result.Should().NotBeNull();
        isUserAdded.Should().BeTrue();
    }
    
    private AddInviteUserRequest CreateInviteUserRequest()
    {
        return new AddInviteUserRequest
        {
            InvitedUser = new()
            {
                Email = "test@abc.com",
                OrganisationId = _existingOrgId,
                PersonRoleId = PersonRole.Employee,
                ServiceRoleId = 3
            },
            InvitingUser = new()
            {
                Email = "inviter@test.com",
                UserId = new Guid("00000003-0003-0003-0003-000000000003")
            }
        };
    }

    [TestMethod]
    public async Task When_User_Is_Enrolled_With_No_Invited_Enrolments_Return_False()
    {
        // Arrange
        AccountManagementTestHelper.SetupDatabaseForEnrolUser(_accountContext);

        _accountContext.Enrolments.Single().EnrolmentStatusId = 
            Data.DbConstants.EnrolmentStatus.Enrolled; // query will no longer find an Enrolment with status of Invited 
        await _accountContext.SaveChangesAsync(default, cancellationToken: default);
        
        var invitedUser = _accountContext.Users.Single(x => x.Id == UserIdToEnroll);
        
        var enrolmentRequest = new EnrolInvitedUserRequest
        {
            UserId = new Guid("00000000-0000-0000-0000-000000000002"),
            Email = "invitee2@test.com",
            InviteToken = "_inviteToken_",
            FirstName = "inviteFirstName",
            LastName = "inviteeLastName"
        };

        // Act
        var success = await _sut.EnrolInvitedUserAsync(invitedUser, enrolmentRequest);

        // Assert
        AssertUserIsNotEnrolled(success);
    }
    
    [TestMethod]
    public async Task When_User_Is_Enrolled_With_No_Enrolments_Return_False()
    {
        // Arrange
        AccountManagementTestHelper.SetupDatabaseForEnrolUser(_accountContext);
        
        var enrolment = _accountContext.Enrolments.Single();
        _accountContext.Remove(enrolment); // query will no longer match with any Enrolments
        await _accountContext.SaveChangesAsync(default, cancellationToken: default);

        var invitedUser = _accountContext.Users.Single(x => x.Id == UserIdToEnroll);
        
        var enrolmentRequest = new EnrolInvitedUserRequest
        {
            UserId = new Guid("00000000-0000-0000-0000-000000000002"),
            Email = "invitee2@test.com",
            InviteToken = "_inviteToken_",
            FirstName = "inviteFirstName",
            LastName = "inviteeLastName"
        };

        // Act
        var success = await _sut.EnrolInvitedUserAsync(invitedUser, enrolmentRequest);

        // Assert
        AssertUserIsNotEnrolled(success);
    }

    private void AssertUserIsNotEnrolled(bool success)
    {
        success.Should().BeFalse();
        
        var user = _accountContext.Users.Include(x => x.Person).Single(x => x.Id == UserIdToEnroll);
        user.InviteToken.Should().Be("_inviteToken_");
        user.Person.FirstName.Should().Be(string.Empty);
        user.Person.LastName.Should().Be(string.Empty);
        user.UserId.Should().Be(Guid.Empty);
    }
    
    [TestMethod]
    [DataRow("fifty_one_characters_123456789012345678901234567890", "last_name")]
    [DataRow("first_name", "fifty_one_characters_123456789012345678901234567890")]
    public async Task User_should_not_be_enrolled_when_names_exceed_max_character_limit(string firstName, string lastName)
    {
        // Arrange
        AccountManagementTestHelper.SetupDatabaseForEnrolUser(_accountContext);
        var invitedUser = _accountContext.Users.Single(x => x.Id == UserIdToEnroll);
        var enrolmentRequest = new EnrolInvitedUserRequest
        {
            UserId = new Guid("00000000-0000-0000-0000-000000000002"),
            Email = "invitee2@test.com",
            InviteToken = "_inviteToken_",
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        await Assert.ThrowsExactlyAsync<ValidationException>(async () => await _sut.EnrolInvitedUserAsync(invitedUser, enrolmentRequest));
    }
    
    [TestMethod]
    public async Task When_User_Is_Enrolled_With_Valid_Data_Return_True()
    {
        // Arrange
        AccountManagementTestHelper.SetupDatabaseForEnrolUser(_accountContext);
        var invitedUser = _accountContext.Users.Single(x => x.Id == UserIdToEnroll);
        var enrolmentRequest = new EnrolInvitedUserRequest
        {
            UserId = new Guid("00000000-0000-0000-0000-000000000002"),
            Email = "invitee2@test.com",
            InviteToken = "_inviteToken_",
            FirstName = "inviteeFirstName",
            LastName = "inviteeLastName"
        };

        // Act
        var success = await _sut.EnrolInvitedUserAsync(invitedUser, enrolmentRequest);

        // Assert
        success.Should().BeTrue();

        var user = _accountContext.Users.Include(x => x.Person).Single(x => x.Id == UserIdToEnroll);
        user.InviteToken.Should().BeNull();
        user.Person.FirstName.Should().Be("inviteeFirstName");
        user.Person.LastName.Should().Be("inviteeLastName");
        user.UserId.Should().Be(new Guid("00000000-0000-0000-0000-000000000002"));
    }

    [TestMethod]
    public async Task When_User_Is_ReInvited_With_Valid_Data_Then_New_Invite_Token_Is_Generated()
    {
        //Arrange
        AccountManagementTestHelper.SetupDatabaseForInvitingUser(_accountContext);
        var initialInviteRequest = CreateInviteUserRequest();
        var initialInviteToken = await _sut.CreateInviteeAccountAsync(initialInviteRequest);
        var invitedPerson = _accountContext.Persons.Single(x => x.Email == initialInviteRequest.InvitedUser.Email);
        var isUserEnrolmentDeleted = await _enrolmentsService.DeleteEnrolmentsForPersonAsync(
            invitedPerson.User.UserId.Value, 
            invitedPerson.ExternalId, 
            invitedPerson.OrganisationConnections.Single().Organisation.ExternalId, 
            1);
        
        //Act
        var reInviteToken =
            await _sut.ReInviteUserAsync(initialInviteRequest.InvitedUser, initialInviteRequest.InvitingUser);
        
        //Assert
        initialInviteToken.Should().NotBeNull().And.NotBeEmpty();
        isUserEnrolmentDeleted.Should().BeTrue();
        reInviteToken.Should().BeOfType(typeof(string));
        reInviteToken.Should().NotBeNull().And.NotBeEmpty();
    }

    [TestMethod]
    public async Task When_User_Is_ReInvited_With_Valid_Data_And_Person_Role_Employee_To_Admin_Then_New_Invite_Token_Is_Generated()
    {
        //Arrange
        AccountManagementTestHelper.SetupDatabaseForInvitingUser(_accountContext);
        var initialInviteRequest = CreateInviteUserRequest();
        var initialInviteToken = await _sut.CreateInviteeAccountAsync(initialInviteRequest);
        var invitedPerson = _accountContext.Persons.Single(x => x.Email == initialInviteRequest.InvitedUser.Email);
        var isUserEnrolmentDeleted = await _enrolmentsService.DeleteEnrolmentsForPersonAsync(
            invitedPerson.User.UserId.Value, 
            invitedPerson.ExternalId, 
            invitedPerson.OrganisationConnections.Single().Organisation.ExternalId, 
            1);
        initialInviteRequest.InvitedUser.PersonRoleId = PersonRole.Admin;

        //Act
        var reInviteToken =
            await _sut.ReInviteUserAsync(initialInviteRequest.InvitedUser, initialInviteRequest.InvitingUser);
        
        //Assert
        invitedPerson.OrganisationConnections.First().PersonRoleId.Should().Be(PersonRole.Admin);
        initialInviteToken.Should().NotBeNull().And.NotBeEmpty();
        isUserEnrolmentDeleted.Should().BeTrue();
        reInviteToken.Should().BeOfType(typeof(string));
        reInviteToken.Should().NotBeNull().And.NotBeEmpty();
    }

    [TestMethod]
    public async Task When_User_Is_ReInvited_With_Valid_Data_And_Person_Role_Admin_To_Employee_Then_New_Invite_Token_Is_Generated()
    {
        //Arrange
        AccountManagementTestHelper.SetupDatabaseForInvitingUser(_accountContext);
        var initialInviteRequest = CreateInviteUserRequest();
        initialInviteRequest.InvitedUser.PersonRoleId = PersonRole.Admin;
        var initialInviteToken = await _sut.CreateInviteeAccountAsync(initialInviteRequest);
        var invitedPerson = _accountContext.Persons.Single(x => x.Email == initialInviteRequest.InvitedUser.Email);
        var isUserEnrolmentDeleted = await _enrolmentsService.DeleteEnrolmentsForPersonAsync(
            invitedPerson.User.UserId.Value, 
            invitedPerson.ExternalId, 
            invitedPerson.OrganisationConnections.Single().Organisation.ExternalId, 
            1);
        initialInviteRequest.InvitedUser.PersonRoleId = PersonRole.Employee;
        //Act
        var reInviteToken =
            await _sut.ReInviteUserAsync(initialInviteRequest.InvitedUser, initialInviteRequest.InvitingUser);
        
        //Assert
        invitedPerson.OrganisationConnections.First().PersonRoleId.Should().Be(PersonRole.Employee);
        initialInviteToken.Should().NotBeNull().And.NotBeEmpty();
        isUserEnrolmentDeleted.Should().BeTrue();
        reInviteToken.Should().BeOfType(typeof(string));
        reInviteToken.Should().NotBeNull().And.NotBeEmpty();
    }

    [TestMethod]
    public async Task ReInviteUser_Throws_When_Invited_Person_IsNot_Invited_Before()
    {
        //Arrange
        AccountManagementTestHelper.SetupDatabaseForInvitingUser(_accountContext);

        var initialInviteRequest = CreateInviteUserRequest();
        var initialInviteToken = await _sut.CreateInviteeAccountAsync(initialInviteRequest);
        
        var invitedPerson = _accountContext.Persons.Single(x => x.Email == initialInviteRequest.InvitedUser.Email);
        var isUserEnrolmentDeleted = await _enrolmentsService.DeleteEnrolmentsForPersonAsync(
            invitedPerson.User.UserId.Value, 
            invitedPerson.ExternalId, 
            invitedPerson.OrganisationConnections.Single().Organisation.ExternalId, 
            1);
        
        //Act
        var reInviteAction = async () =>
            await _sut.ReInviteUserAsync(new InvitedUser
            {
                Email = "not.invited.before@test.com",
                OrganisationId = initialInviteRequest.InvitedUser.OrganisationId,
                PersonRoleId = initialInviteRequest.InvitedUser.PersonRoleId,
                ServiceRoleId = initialInviteRequest.InvitedUser.ServiceRoleId,
                UserId = initialInviteRequest.InvitedUser.UserId
            },
            initialInviteRequest.InvitingUser);
        
        //Assert
        initialInviteToken.Should().NotBeNull().And.NotBeEmpty();
        isUserEnrolmentDeleted.Should().BeTrue();
        await reInviteAction.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task ReInviteUser_Throws_When_Invited_Person_IsNot_In_Same_Organisation()
    {
        //Arrange
        AccountManagementTestHelper.SetupDatabaseForInvitingUser(_accountContext);

        var initialInviteRequest = CreateInviteUserRequest();
        await _sut.CreateInviteeAccountAsync(initialInviteRequest);
        
        var invitedPerson = _accountContext.Persons.Single(x => x.Email == initialInviteRequest.InvitedUser.Email);
        await _enrolmentsService.DeleteEnrolmentsForPersonAsync(
            invitedPerson.User.UserId.Value, 
            invitedPerson.ExternalId, 
            invitedPerson.OrganisationConnections.Single().Organisation.ExternalId, 
            1);

        var differentOrganisationId = new Guid("00000000-0000-0000-0000-000000000003");

        //Act & Assert
        await Assert.ThrowsExactlyAsync<ValidationException>(async () => await _sut.ReInviteUserAsync(new InvitedUser
            {
                Email = initialInviteRequest.InvitedUser.Email,
                OrganisationId = differentOrganisationId,
                PersonRoleId = initialInviteRequest.InvitedUser.PersonRoleId,
                ServiceRoleId = initialInviteRequest.InvitedUser.ServiceRoleId,
                UserId = initialInviteRequest.InvitedUser.UserId
            },
            initialInviteRequest.InvitingUser));

    }
}