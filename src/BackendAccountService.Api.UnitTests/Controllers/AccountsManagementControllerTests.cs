using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class AccountsManagementControllerTests
{
    private AccountsManagementController _accountsManagementController = null!;
    private readonly Mock<IAccountManagementService> _accountManagementServiceMock = new();
    private readonly Mock<IValidationService> _validateDataService = new();
    private readonly Mock<IEnrolmentsService> _enrolmentsServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock =new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly NullLogger<AccountsManagementController> _nullLogger = new();
    private const string InviteToken = "SomeInviteToken";
    private const string ReInviteToken = "SomeReInviteToken";
    private readonly Guid _existingOrgId = new Guid("00000000-0000-0000-0000-000000000001");
    
    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _accountsManagementController = new AccountsManagementController(_accountManagementServiceMock.Object, _nullLogger,
            _apiConfigOptionsMock.Object, _validateDataService.Object, _userServiceMock.Object, _enrolmentsServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
    
    [TestMethod]
    public async Task When_Invite_User_Is_Requested_With_Valid_Data_And_User_Is_Authorised_Then_Return_Token_As_Response()
    {
        // Arrange
        var request = CreateInviteUserRequest();
        
        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ReturnsAsync(InviteToken);
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(false);
        _validateDataService
            .Setup(service => service.IsAuthorisedToManageUsers(
                request.InvitingUser.UserId, request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(true);

        // Act
        var result = await _accountsManagementController.InviteUser(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(InviteToken);
    }
    
    [TestMethod]
    public async Task When_Invite_User_Is_Requested_With_Valid_Data_And_User_Is_Not_Authorised_Then_Return_Token_As_Response()
    {
        // Arrange
        var request = CreateInviteUserRequest();
        
        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ReturnsAsync(InviteToken);
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(false);
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(request.InvitingUser.UserId,
                request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(false);

        // Act
        var result = await _accountsManagementController.InviteUser(request) as ActionResult;

        // Assert
        result.Should().BeOfType<ObjectResult>();
        (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }
   
    [TestMethod]
    public async Task When_Invite_User_Is_Requested_And_Exception_Occurs_Then_Return_Problem()
    {
        // Arrange
        var request = CreateInviteUserRequest();
        
        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ThrowsAsync(new Exception());
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(false);
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(request.InvitingUser.UserId,
                request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(true);
       
        // Act
        var result =  async () => await _accountsManagementController.InviteUser(request) as ActionResult;

        // Assert
        await result.Should().ThrowExactlyAsync<Exception>();
    }

    [TestMethod]
    public async Task When_Invite_User_Is_Requested_And_AccountManagementService_Fails_On_CreateInviteeAccountAsync_Then_Return_Problem()
    {
        // Arrange
        var request = CreateInviteUserRequest();
        
        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ReturnsAsync(string.Empty);
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(false);
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(request.InvitingUser.UserId,
                request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(true);

        // Act
        var result = await _accountsManagementController.InviteUser(request) as ObjectResult;
    
        // Assert
        var problemDetails = result?.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails.Type.Should().Contain("https://dummytest/token not generated");
    }

    [TestMethod]
    public async Task When_User_ReInvited_And_They_WereNot_Removed_Before_Then_Return_Problem()
    {
        // Arrange
        var request = CreateInviteUserRequest();
    
        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ReturnsAsync(InviteToken);
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(true);
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(request.InvitingUser.UserId,
                request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(true);
        _enrolmentsServiceMock.Setup(x =>
                x.IsUserEnrolledAsync(request.InvitedUser.Email, request.InvitedUser.OrganisationId))
            .ReturnsAsync(true);

        // Act
        var result = await _accountsManagementController.InviteUser(request) as ObjectResult;
    
        // Assert
        result.Should().NotBeNull();
        var validationProblemDetails = result.Value as ValidationProblemDetails;
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.FirstOrDefault().Key.Should().Be("Email");
        validationProblemDetails.Errors.FirstOrDefault().Value.First().Should()
            .Be("Invited user 'test@abc.com' is enrolled already.");
    }

    [TestMethod]
    public async Task When_User_ReInvited_And_They_WereNot_From_Same_Organisation_Then_Return_Problem()
    {
        // Arrange
        var request = CreateInviteUserRequest();
    
        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ReturnsAsync(InviteToken);
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(true);
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(request.InvitingUser.UserId,
                request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(true);
        _enrolmentsServiceMock.Setup(x =>
                x.IsUserEnrolledAsync(request.InvitedUser.Email, request.InvitedUser.OrganisationId))
            .ReturnsAsync(false);
        _userServiceMock.Setup(x => x.GetUserOrganisationAsync(request.InvitingUser.UserId))
            .ReturnsAsync(new Result<UserOrganisationsListModel>(true, new UserOrganisationsListModel
            {
                User = new UserDetailsModel
                { Organisations = new List<OrganisationDetailModel> { new() { Id = Guid.NewGuid() } } }
            }, string.Empty, HttpStatusCode.OK));

        // Act
        var result = await _accountsManagementController.InviteUser(request) as ObjectResult;
    
        // Assert
        result.Should().NotBeNull();
        var validationProblemDetails = result.Value as ValidationProblemDetails;
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.FirstOrDefault().Key.Should().Be("Email");
        validationProblemDetails.Errors.FirstOrDefault().Value.First().Should()
            .Be("Invited user 'test@abc.com' doesn't belong to the same organisation.");
    }

    [TestMethod]
    public async Task When_User_ReInvited_And_AccountManagementService_Fails_On_ReInviteUserAsync_Then_Return_Problem()
    {
        // Arrange
        var request = CreateInviteUserRequest();
    
        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ReturnsAsync(InviteToken);
        _accountManagementServiceMock.Setup(service => service.ReInviteUserAsync(request.InvitedUser, request.InvitingUser))
            .ReturnsAsync(string.Empty);
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(true);
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(request.InvitingUser.UserId,
                request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(true);
        _enrolmentsServiceMock.Setup(x =>
                x.IsUserEnrolledAsync(request.InvitedUser.Email, request.InvitedUser.OrganisationId))
            .ReturnsAsync(false);
        _userServiceMock.Setup(x => x.GetUserOrganisationAsync(request.InvitingUser.UserId))
            .ReturnsAsync(new Result<UserOrganisationsListModel>(true, new UserOrganisationsListModel
            {
                User = new UserDetailsModel
                {
                    Organisations = new List<OrganisationDetailModel>
                        { new() { Id = request.InvitedUser.OrganisationId } }
                }
            }, string.Empty, HttpStatusCode.OK));

        // Act
        var result = await _accountsManagementController.InviteUser(request) as ObjectResult;
    
        // Assert
        var problemDetails = result?.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails.Type.Should().Contain("https://dummytest/token not generated");
    }

    [TestMethod]
    public async Task When_User_ReInvited_And_Removed_Before_And_From_Same_Organisation_Then_Return_Ok()
    {
        // Arrange
        var request = CreateInviteUserRequest();
    
        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ReturnsAsync(InviteToken);
        _accountManagementServiceMock.Setup(service => service.ReInviteUserAsync(request.InvitedUser, request.InvitingUser))
            .ReturnsAsync(ReInviteToken);
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(true);
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(request.InvitingUser.UserId,
                request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(true);
        _enrolmentsServiceMock.Setup(x =>
                x.IsUserEnrolledAsync(request.InvitedUser.Email, request.InvitedUser.OrganisationId))
            .ReturnsAsync(false);
        _userServiceMock.Setup(x => x.GetUserOrganisationAsync(request.InvitingUser.UserId))
            .ReturnsAsync(new Result<UserOrganisationsListModel>(true, new UserOrganisationsListModel
            {
                User = new UserDetailsModel
                {
                    Organisations = new List<OrganisationDetailModel>
                        { new() { Id = request.InvitedUser.OrganisationId } }
                }
            }, string.Empty, HttpStatusCode.OK));

        // Act
        var result = await _accountsManagementController.InviteUser(request) as OkObjectResult;
    
        // Assert
        var problemDetails = result?.Value as string;
        problemDetails.Should().NotBeNull();
        problemDetails.Should().Contain(ReInviteToken);
    }
    
    private AddInviteUserRequest CreateInviteUserRequest()
    {
        return new AddInviteUserRequest
        {
            InvitedUser = new()
            {
                Email = "test@abc.com",
                OrganisationId = _existingOrgId,
                PersonRoleId = 2,
                ServiceRoleId = 3
            },
            InvitingUser = new()
            {
                Email = "inviter@test.com",
                UserId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2")
            }
        };
    }
    
    [TestMethod]
    public async Task When_Model_Is_Invalid_Then_Return_ValidationProblem()
    {
        // Arrange
        var request = new EnrolInvitedUserRequest();
        _accountsManagementController.ModelState.AddModelError("test1", "test2");

        // Act
        var result = await _accountsManagementController.EnrolInvitedUser(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        var validationProblemDetails = result.Value as ValidationProblemDetails;
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.FirstOrDefault().Key.Should().Be("test1");
        validationProblemDetails.Errors.FirstOrDefault().Value.First().Should().Be("test2");
    }

    [TestMethod]
    public async Task When_No_Matching_Email_And_Invite_Token_Is_Found_Then_Return_BadRequest()
    {
        // Arrange
        var request = new EnrolInvitedUserRequest();
        _userServiceMock.Setup(x =>
                x.GetUserByInviteAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((User?) null);
        
        // Act
        var result = await _accountsManagementController.EnrolInvitedUser(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task When_No_Enrolments_Exist_For_User_Return_BadRequest()
    {
        // Arrange
        var request = new EnrolInvitedUserRequest();
        var user = new User();
        _userServiceMock.Setup(x =>
                x.GetUserByInviteAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(user);
        _accountManagementServiceMock.Setup(x =>
                x.EnrolInvitedUserAsync(It.IsAny<User>(), It.IsAny<EnrolInvitedUserRequest>()))
            .ReturnsAsync(false);
        
        // Act
        var result = await _accountsManagementController.EnrolInvitedUser(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [TestMethod]
    public async Task When_Enrolments_Is_Successful_Return_NoContent()
    {
        // Arrange
        var request = new EnrolInvitedUserRequest();
        var user = new User();
        _userServiceMock.Setup(x =>
                x.GetUserByInviteAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(user);
        _accountManagementServiceMock.Setup(x =>
                x.EnrolInvitedUserAsync(It.IsAny<User>(), It.IsAny<EnrolInvitedUserRequest>()))
            .ReturnsAsync(true);
        
        // Act
        var result = await _accountsManagementController.EnrolInvitedUser(request) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }
}