using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class RegulatorAccountsControllerTests
{
    private RegulatorAccountsController _regulatorAccountsController = null!;
    private readonly Mock<IAccountManagementService> _accountManagementServiceMock = new();
    private readonly Mock<IValidationService> _validateDataService = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<RegulatorAccountsController> _nullLogger = new();
        
    private const string InviteToken = "SomeInviteToken";
    private readonly Guid _existingOrgId = new("00000000-0000-0000-0000-000000000001");

    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _regulatorAccountsController = new RegulatorAccountsController(
            _apiConfigOptionsMock.Object, 
            _accountManagementServiceMock.Object,
            _validateDataService.Object, 
            _nullLogger)                 
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task InviteUser_WhenValidRequestIsSent_ThenReturnToken()
    {
        // Arrange
        var request = CreateInviteUserRequest();

        _accountManagementServiceMock.Setup(service => service.CreateInviteeAccountAsync(request))
            .ReturnsAsync(InviteToken);
            
        _validateDataService.Setup(service => service.IsUserInvitedAsync(request.InvitedUser.Email))
            .ReturnsAsync(false);
            
        _validateDataService.Setup(service => service.IsAuthorisedToManageUsers(request.InvitingUser.UserId, request.InvitedUser.OrganisationId, request.InvitedUser.ServiceRoleId))
            .Returns(true);

        // Act
        var result = await _regulatorAccountsController.InviteUser(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(InviteToken);
    }

    [TestMethod]
    public async Task InviteUser_WhenInviteAlreadyExists_ReturnError()
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

        // Act
        var result = await _regulatorAccountsController.InviteUser(request) as ObjectResult;

        // Assert
        var validationProblemDetails = result?.Value as ValidationProblemDetails;
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails?.Type.Should().Contain("https://dummytest/validation");
        validationProblemDetails?.Errors.FirstOrDefault().Key.Should().Contain(nameof(request.InvitedUser.Email));
    }

    [TestMethod]
    public async Task InviteUser_WhenExceptionOccurs_ThenReturnError()
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
        var result = async () => await _regulatorAccountsController.InviteUser(request) as ActionResult;

        // Assert
        await result.Should().ThrowExactlyAsync<Exception>();
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
    public async Task InvitedUser_WhenValidRequestInvitationExists_ThenReturnToken()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string validEmail = "valid.test@test.com";
        string expectedToken = "inviteToken";

        _validateDataService.Setup(service => service.UserInvitedTokenAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _regulatorAccountsController.InvitedUser(userId, validEmail) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        Assert.AreEqual(result.StatusCode, (int)HttpStatusCode.OK);
        result.Value.Should().BeEquivalentTo(expectedToken);
    }

    [TestMethod]
    public async Task InvitedUser_WhenValidRequestInvitationDoesNotExist_ThenReturnEmptyString()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string invalidEmail = "invalid.test@test.com";
        string expectedToken = string.Empty;

        _validateDataService.Setup(service => service.UserInvitedTokenAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _regulatorAccountsController.InvitedUser(userId, invalidEmail) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(result.StatusCode, (int)HttpStatusCode.OK);
        result.Value.Should().Be(expectedToken);
    }
}