using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Core.Models.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class PersonsControllerTests
{
    private PersonsController _personsController = null!;
    private Mock<IPersonService> _personServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _personServiceMock = new Mock<IPersonService>();
        _userServiceMock = new Mock<IUserService>();
        _apiConfigOptionsMock = new Mock<IOptions<ApiConfig>>();

        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _personsController = new PersonsController(
            _personServiceMock.Object,
            _apiConfigOptionsMock.Object,
            _userServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task CreateAccount_WhenUserExists_ReturnsOk()
    {
        var userId = Guid.NewGuid();    

        _personServiceMock
            .Setup(service => service.GetPersonResponseByUserId(userId))
            .ReturnsAsync(new PersonResponseModel());

        var result = await _personsController.GetPersonByUserId(userId) as OkObjectResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<PersonResponseModel>();
    }

    [TestMethod]
    public async Task GetAllPersonByUserId_WhenUserExists_ReturnsOk()
    {
        var userId = Guid.NewGuid();

        _personServiceMock
            .Setup(service => service.GetAllPersonByUserIdAsync(userId))
            .ReturnsAsync(new PersonResponseModel());

        var result = await _personsController.GetAllPersonByUserId(userId) as OkObjectResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<PersonResponseModel>();
    }

    [TestMethod]
    public async Task CreateAccount_WhenUserDoesNotExist_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();

        _personServiceMock
            .Setup(service => service.GetPersonResponseByUserId(userId))
            .ReturnsAsync((PersonResponseModel?)null);

        var result = await _personsController.GetPersonByUserId(userId) as NoContentResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetAllPersonByUserId_WhenUserDoesNotExist_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();

        _personServiceMock
            .Setup(service => service.GetAllPersonByUserIdAsync(userId))
            .ReturnsAsync((PersonResponseModel?)null);

        var result = await _personsController.GetAllPersonByUserId(userId) as NoContentResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task PersonExternalId_WhenUserExists_ReturnsOk()
    {
        var externalId = Guid.NewGuid();    

        _personServiceMock
            .Setup(service => service.GetPersonByExternalIdAsync(externalId))
            .ReturnsAsync(new PersonResponseModel());

        var result = await _personsController.GetPersonByExternalIdAsync(externalId) as OkObjectResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<PersonResponseModel>();
    }
    
    [TestMethod]
    public async Task PersonExternalId_WhenUserDoesNotExist_ReturnsNoContent()
    {
        var externalId = Guid.NewGuid();

        _personServiceMock
            .Setup(service => service.GetPersonByExternalIdAsync(externalId))
            .ReturnsAsync((PersonResponseModel?)null);

        var result = await _personsController.GetPersonByExternalIdAsync(externalId) as NoContentResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }
    
    [TestMethod]
    public async Task GetPersonByInviteTokenAsync_WhenUserDoesNotExist_ReturnsNoContent()
    {
        var token = "some invike token";

        _personServiceMock
            .Setup(service => service.GetPersonServiceRoleByInviteTokenAsync(token))
            .ReturnsAsync((InviteApprovedUserModel)null);

        var result = await _personsController.GetPersonByInviteTokenAsync(token) as NoContentResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }
    
    [TestMethod]
    public async Task GetPersonByInviteTokenAsync_ReturnsOk_WithInviteApprovedUserModel()
    {
        // Arrange
        var token = "validToken";
        _personServiceMock.Setup(service => service.GetPersonServiceRoleByInviteTokenAsync(token))
            .ReturnsAsync((InviteApprovedUserModel)null); 
        _userServiceMock.Setup(service => service.InvitationTokenExists(token))
            .ReturnsAsync(true);

        // Act
        var result = await _personsController.GetPersonByInviteTokenAsync(token);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var inviteApprovedUserModel = okResult!.Value as InviteApprovedUserModel;
        inviteApprovedUserModel.Should().NotBeNull();
        inviteApprovedUserModel!.IsInvitationTokenInvalid.Should().BeTrue();
    }
}
