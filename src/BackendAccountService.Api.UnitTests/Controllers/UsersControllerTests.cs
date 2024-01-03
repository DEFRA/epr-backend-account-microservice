using System.Net;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class UsersControllerTests
{
    private UsersController _userController = null!;
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<UsersController> _nullLogger = new();
    private readonly Guid _userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");

    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _userController = new UsersController(_userServiceMock.Object, _apiConfigOptionsMock.Object, _nullLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task GetOrganisationListForUser_ReturnsOk()
    {
        // Arrange
        var userOrganisationsListModel = new UserOrganisationsListModel
        {
            User = new UserDetailsModel
            {
                Id = _userId
            }
        };
        var response = Result<UserOrganisationsListModel>.SuccessResult(userOrganisationsListModel);

        _userServiceMock.Setup(service => service.GetUserOrganisationAsync(_userId))
            .ReturnsAsync(response);

        // Act
        var result = await _userController.GetUserOrganisation(_userId) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetListOfUserOrganisations_ReturnsException()
    {
        // Arrange
        _userServiceMock.Setup(service => service.GetUserOrganisationAsync(_userId))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _userController.GetUserOrganisation(_userId) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetListOfUserOrganisations_WhenUserIdIdNotPresent_ReturnsNotFound()
    {
        // Arrange
        var response = Result<UserOrganisationsListModel>.FailedResult("No organisation found for the user", HttpStatusCode.NotFound);

        _userServiceMock.Setup(service => service.GetUserOrganisationAsync(_userId))
            .ReturnsAsync(response);

        // Act
        var result = await _userController.GetUserOrganisation(_userId) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetPersonOrganisations_ReturnsOk()
    {
        // Arrange
        var personWithOrganisationsResponse = It.IsAny<PersonWithOrganisationsResponse>();
        var response = Result<PersonWithOrganisationsResponse>.SuccessResult(personWithOrganisationsResponse);

        _userServiceMock.Setup(service => service.GetPersonOrganisationsWithEnrolmentsForServiceAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userController.GetPersonOrganisations(It.IsAny<Guid>(), It.IsAny<string>()) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetPersonOrganisations_ReturnsException()
    {
        // Arrange
        _userServiceMock.Setup(service => service.GetPersonOrganisationsWithEnrolmentsForServiceAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(new Result<PersonWithOrganisationsResponse?>(false,
                It.IsAny<PersonWithOrganisationsResponse>(),
                "ErrorMessage",
                HttpStatusCode.InternalServerError));

        // Act
        var result = await _userController.GetPersonOrganisations(It.IsAny<Guid>(), It.IsAny<string>()) as OkObjectResult;

        // Assert
        result?.Value.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetPersonOrganisations_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var response = Result<PersonWithOrganisationsResponse>.FailedResult("UserDoesNotExist", HttpStatusCode.NotFound);

        _userServiceMock.Setup(service => service.GetPersonOrganisationsWithEnrolmentsForServiceAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userController.GetPersonOrganisations(It.IsAny<Guid>(), It.IsAny<string>()) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
}