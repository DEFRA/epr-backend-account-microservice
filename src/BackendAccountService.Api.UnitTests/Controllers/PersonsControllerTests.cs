using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Core.Models.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class PersonsControllerTests
{
    private PersonsController _personsController = null!;
    private Mock<IPersonService> _personServiceMock = null!;
    private Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = null!;


    [TestInitialize]
    public void Setup()
    {
        _personServiceMock = new Mock<IPersonService>();

        _apiConfigOptionsMock = new Mock<IOptions<ApiConfig>>();

        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _personsController = new PersonsController(_personServiceMock.Object, _apiConfigOptionsMock.Object)
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
            .Setup(service => service.GetPersonByUserIdAsync(userId))
            .ReturnsAsync(new PersonResponseModel());

        var result = await _personsController.GetPersonByUserId(userId) as OkObjectResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<PersonResponseModel>();
    }

    [TestMethod]
    public async Task CreateAccount_WhenUserDoesNotExist_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();

        _personServiceMock
            .Setup(service => service.GetPersonByUserIdAsync(userId))
            .ReturnsAsync((PersonResponseModel?)null);

        var result = await _personsController.GetPersonByUserId(userId) as NoContentResult;

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
    public async Task GetPersonByInviteTokenAsync_WhenUserDoesNotExist_ReturnsOk()
    {
        var token = "some invike token";

        _personServiceMock
            .Setup(service => service.GetPersonServiceRoleByInviteTokenAsync(token))
            .ReturnsAsync((InviteApprovedUserModel)null);

        var result = await _personsController.GetPersonByInviteTokenAsync(token) as NoContentResult;

        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }
}
