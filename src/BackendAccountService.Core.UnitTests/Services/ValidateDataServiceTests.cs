using BackendAccountService.Core.Services;
using BackendAccountService.Core.UnitTests.TestHelpers;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class ValidateDataServiceTests
{
    private AccountsDbContext _accountContext = null!;
    private ValidationService _validationService = null!;
    private readonly NullLogger<ValidationService> _logger = new();
    
    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _accountContext = new AccountsDbContext(contextOptions);
        _validationService = new ValidationService(_accountContext, _logger);
    }
    
    [TestMethod]
    public async Task When_User_Is_Already_Invited_Then_Return_True()
    {
        //Setup
        AccountManagementTestHelper.SetupDatabaseForInviteUser(_accountContext);
        var email = "invitee@test.com";
      
        //Act
        var result = await _validationService.IsUserInvitedAsync(email);
       
        //Assert
        result.Should().BeTrue();
    }
    
     
    [TestMethod]
    public async Task When_User_Is_Not_Invited_Already_Then_Return_False()
    {
        //Setup
        AccountManagementTestHelper.SetupDatabaseForInviteUser(_accountContext);
        var email = "invitee2@test.com";
      
        //Act
        var result = await _validationService.IsUserInvitedAsync(email);
       
        //Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("approvedperson1@test.com","delegatedperson1@test.com")]
    [DataRow("approvedperson1@test.com","basicadmin1@test.com")]
    [DataRow("approvedperson1@test.com","basicuser1@test.com")]
    [DataRow("delegatedperson1@test.com","basicadmin1@test.com")]
    [DataRow("delegatedperson1@test.com","basicuser1@test.com")]
    [DataRow("basicadmin1@test.com","basicadmin2@test.com")]
    [DataRow("basicadmin1@test.com","basicuser1@test.com")]
    public async Task IsAuthorisedToRemoveEnrolledUser_When_User_Is_Enrolled_And_Is_Authorised_Then_Return_True(
        string loggedInUserEmail, string personToRemoveEmail)
    {
        //Setup
        EnrolmentsTestHelper.SetUpDatabase(_accountContext);
        
        var loggedInUserId = _accountContext.Users.Single(x=> x.Email == loggedInUserEmail).UserId.Value;
        var personId = _accountContext.Persons.Single(x=> x.Email == personToRemoveEmail).ExternalId;
        var organisationId = _accountContext.Organisations.Single(x=> x.Name == "organisation1").ExternalId;
        var serviceRoleId = 1;
      
        //Act
        var result = _validationService.IsAuthorisedToRemoveEnrolledUser(
            loggedInUserId, 
            organisationId, 
            serviceRoleId,
            personId);
       
        //Assert
        result.Should().BeTrue();
    }
    
    [TestMethod]
    [DataRow("approvedperson1@test.com","approvedperson2@test.com")]
    [DataRow("delegatedperson1@test.com","approvedperson1@test.com")]
    [DataRow("delegatedperson1@test.com","delegatedperson2@test.com")]
    [DataRow("basicadmin1@test.com","approvedperson1@test.com")]
    [DataRow("basicadmin1@test.com","delegatedperson1@test.com")]
    [DataRow("basicuser1@test.com","approvedperson1@test.com")]
    [DataRow("basicuser1@test.com","delegatedperson1@test.com")]
    [DataRow("basicuser1@test.com","basicadmin1@test.com")]
    [DataRow("basicuser1@test.com","basicuser2@test.com")]
    public async Task IsAuthorisedToRemoveEnrolledUser_When_User_Is_Enrolled_But_Not_Authorised_Then_Return_False(
        string loggedInUserEmail, string personToRemoveEmail)
    {
        //Setup
        EnrolmentsTestHelper.SetUpDatabase(_accountContext);
        
        var loggedInUserId = _accountContext.Users.Single(x=> x.Email == loggedInUserEmail).UserId.Value;
        var personId = _accountContext.Persons.Single(x=> x.Email == personToRemoveEmail).ExternalId;
        var organisationId = _accountContext.Organisations.Single(x=> x.Name == "organisation1").ExternalId;
        var serviceRoleId = 1;
      
        //Act
        var result = _validationService.IsAuthorisedToRemoveEnrolledUser(
            loggedInUserId, 
            organisationId, 
            serviceRoleId,
            personId);
       
        //Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task IsAuthorisedToRemoveEnrolledUser_When_Person_In_Another_Org_Then_Return_False()
    {
        //Setup
        EnrolmentsTestHelper.SetUpDatabase(_accountContext);

        var loggedInUserId = _accountContext.Users.Single(x => x.Email == "approvedperson1@test.com").UserId.Value;
        var personId = _accountContext.Persons.Single(x => x.Email == "basicuseranotherorg@test.com").ExternalId;
        var organisationId = _accountContext.Organisations.Single(x => x.Name == "organisation1").ExternalId;
        var serviceRoleId = 1;
        
        //Act
        var result = _validationService.IsAuthorisedToRemoveEnrolledUser(
            loggedInUserId, 
            organisationId, 
            serviceRoleId,
            personId);
       
        //Assert
        result.Should().BeFalse();
    }
    
    [TestMethod]
    [DataRow("basicadmin1@test.com")]
    [DataRow("basicadmin2@test.com")]
    [DataRow("delegatedperson1@test.com")]
    [DataRow("delegatedperson2@test.com")]
    [DataRow("approvedperson1@test.com")]
    [DataRow("approvedperson2@test.com")]
    public async Task IsAuthorisedToRemoveEnrolledUser_WHen_User_Tries_To_Remove_Own_Enrollments_Return_False(string userEmail)
    {
        //Setup
        EnrolmentsTestHelper.SetUpDatabase(_accountContext);

        var loggedInUserId = _accountContext.Users.Single(x => x.Email == userEmail).UserId.Value;
        var personId = _accountContext.Persons.Single(x => x.Email == userEmail).ExternalId;
        var organisationId = _accountContext.Organisations.Single(x => x.Name == "organisation1").ExternalId;
        var serviceRoleId = 1;
        
        //Act
        var result = _validationService.IsAuthorisedToRemoveEnrolledUser(
            loggedInUserId, 
            organisationId, 
            serviceRoleId,
            personId);
       
        //Assert
        result.Should().BeFalse();
    }
    
    [TestMethod]
    [DataRow("regulatorAdmin@test.com", true)]
    [DataRow("regulatorBasic@test.com", false)]
    [DataRow("packagingApproved@test.com", false)]
    [DataRow("packagingDelegated@test.com", false)]
    [DataRow("packagingAdmin@test.com", false)]
    [DataRow("packagingUser@test.com", false)]
    public async Task IsAuthorisedToRemoveEnrolledUser_For_Regulator_Returns_Expected_Result(string requestingUser, bool expectedResult)
    {
        //Setup
        EnrolmentsTestHelper.SetUpRegulatorDatabase(_accountContext);

        var loggedInUserId = _accountContext.Users.Single(x => x.Email == requestingUser).UserId.Value;
        var personId = _accountContext.Persons.Single(x => x.Email == "regulatorBasic2@test.com").ExternalId;
        var organisationId = _accountContext.Organisations.Single(x => x.Name == "regulatorOrg").ExternalId;
        var serviceRoleId = 4;
        
        //Act
        var result = _validationService.IsAuthorisedToRemoveEnrolledUser(
            loggedInUserId, 
            organisationId, 
            serviceRoleId,
            personId);
       
        //Assert
        result.Should().Be(expectedResult);
    }
    
    [TestMethod]
    [DataRow("regulatorAdmin@test.com", true)]
    [DataRow("regulatorBasic@test.com", false)]
    [DataRow("packagingApproved@test.com", false)]
    [DataRow("packagingDelegated@test.com", false)]
    [DataRow("packagingAdmin@test.com", false)]
    [DataRow("packagingUser@test.com", false)]
    public async Task IsAuthorisedToManageUsersFromOrganisationForService_For_Regulator_Returns_Expected_Result(string requestingUser, bool expectedResult)
    {
        //Setup
        EnrolmentsTestHelper.SetUpRegulatorDatabase(_accountContext);

        var loggedInUserId = _accountContext.Users.Single(x => x.Email == requestingUser).UserId.Value;
        var organisationId = _accountContext.Organisations.Single(x => x.Name == "regulatorOrg").ExternalId;

        //Act
        var result = await _validationService.IsAuthorisedToManageUsersFromOrganisationForService(
            loggedInUserId, 
            organisationId, 
            "Regulating");
       
        //Assert
        result.Should().Be(expectedResult);
    }
    
    [TestMethod]
    [DataRow("regulatorAdmin@test.com", true)]
    [DataRow("regulatorBasic@test.com", false)]
    [DataRow("packagingApproved@test.com", false)]
    [DataRow("packagingDelegated@test.com", false)]
    [DataRow("packagingAdmin@test.com", false)]
    [DataRow("packagingUser@test.com", false)]
    public async Task IsAuthorisedToManageUsers_For_Regulator_Returns_Expected_Result(string requestingUser, bool expectedResult)
    {
        //Setup
        EnrolmentsTestHelper.SetUpRegulatorDatabase(_accountContext);

        var loggedInUserId = _accountContext.Users.Single(x => x.Email == requestingUser).UserId.Value;
        var organisationId = _accountContext.Organisations.Single(x => x.Name == "regulatorOrg").ExternalId;
        var serviceRoleId = 4;
        
        //Act
        var result = _validationService.IsAuthorisedToManageUsers(
            loggedInUserId, 
            organisationId, 
            serviceRoleId);
       
        //Assert
        result.Should().Be(expectedResult);
    }
}