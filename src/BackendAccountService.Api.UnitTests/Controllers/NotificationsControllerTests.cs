using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class NotificationsControllerTests
{
    private const string ServiceKey = "Packaging";
    private NotificationsController _notificationsController = null!;
    private readonly Mock<INotificationsService> _notificationServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();

    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _notificationsController = new NotificationsController(_notificationServiceMock.Object, _apiConfigOptionsMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    [TestCategory("GetNotifications")]
    public async Task GetNotifications_WhenNoNominationsExist_ThenReturnsNoContentResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var response = new NotificationsResponse { Notifications = new List<Notification>() };

        _notificationServiceMock.Setup(service => service.GetNotificationsForServiceAsync(userId, organisationId, ServiceKey))
            .ReturnsAsync(response);

        // Act
        var result = await _notificationsController.GetNotifications(ServiceKey, userId, organisationId);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result.Result as NoContentResult;
        noContentResult.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    [TestCategory("GetNotifications")]
    public async Task GetNotifications_WhenNotificationsExist_ThenReturnsNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var response = new NotificationsResponse
        {
            Notifications = new List<Notification>
            {
                new() { Type = "Test", Data = new List<KeyValuePair<string, string>> { new("SomeKey", "SomeValue") } }
            }
        };

        _notificationServiceMock.Setup(service => service.GetNotificationsForServiceAsync(userId, organisationId, ServiceKey))
            .ReturnsAsync(response);

        // Act
        var result = await _notificationsController.GetNotifications(ServiceKey, userId, organisationId);

        // Assert
        result.Should().NotBeNull();
        var okObjectResult = result.Result as OkObjectResult;
        okObjectResult.Value.Should().BeEquivalentTo(response);
    }
}