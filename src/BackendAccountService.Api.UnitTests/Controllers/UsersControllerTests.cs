using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class UsersControllerTests
{
    private UsersController _userController = null!;
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<UsersController> _nullLogger = new();
    private readonly Guid _userId = new Guid("5dc5267b-ed00-4551-9129-4abc9944aca2");
    private readonly Guid _organisationId = new Guid("9ddc13d9-9518-4c45-9554-2904396079da");

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

        _userServiceMock.Setup(service => service.GetUserOrganisationAsync(_userId, false))
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
        _userServiceMock.Setup(service => service.GetUserOrganisationAsync(_userId, false))
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

        _userServiceMock.Setup(service => service.GetUserOrganisationAsync(_userId, false))
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

    [TestMethod]
    public async Task GetUserIdForPersonId_ReturnsOk()
    {
        // Arrange
        var response = Guid.NewGuid();

        _userServiceMock.Setup(service => service.GetUserIdByPersonId(It.IsAny<Guid>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userController.GetUserIdByPersonId(It.IsAny<Guid>()) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Value.Should().Be(response);
    }

    [TestMethod]
    public async Task GetUserIdForPersonId_ReturnsException()
    {
        // Arrange
        _userServiceMock.Setup(service => service.GetUserIdByPersonId(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _userController.GetUserIdByPersonId(It.IsAny<Guid>()) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetUserIdForPersonId_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _userServiceMock.Setup(service => service.GetUserIdByPersonId(It.IsAny<Guid>()))
            .ReturnsAsync(default(Guid?));

        // Act
        var result = await _userController.GetUserIdByPersonId(It.IsAny<Guid>()) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task UpdatePersonalDetails_ReturnsOk()
    {
        // Arrange
        var updateUserDetailsRequest = new UpdateUserDetailsRequest { FirstName = "firstname" };
        var updateUserDetailsResponse = It.IsAny<UpdateUserDetailsResponse>();
        var response = Result<UpdateUserDetailsResponse>.SuccessResult(updateUserDetailsResponse);

        _userServiceMock.Setup(service => service.UpdateUserDetailsRequest(It.IsAny<Guid>(), It.IsAny<Guid>(), "Packaging", updateUserDetailsRequest)).ReturnsAsync(response);

        // Act
        var result = await _userController.UpdatePersonalDetails("Packaging", updateUserDetailsRequest, It.IsAny<Guid>(), It.IsAny<Guid>()) as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task UpdatePersonalDetails_Returns_BadRequest_Error_When_NonPackaging_Passed()
    {
        // Arrange
        var updateUserDetailsRequest = new UpdateUserDetailsRequest { FirstName = "firstname" };

        // Act
        var result = await _userController.UpdatePersonalDetails("NonPackaging", updateUserDetailsRequest, It.IsAny<Guid>(), It.IsAny<Guid>()) as ObjectResult;

        // Assert
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
    }

    [TestMethod]
    public async Task UpdatePersonalDetails_Returns_BadRequest_Error_When_NullRequest_Passed()
    {
        // Act
        var result = await _userController.UpdatePersonalDetails("Packaging", null, It.IsAny<Guid>(), It.IsAny<Guid>()) as ObjectResult;

        // Assert
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        ((ProblemDetails)result.Value).Title.Should().Be("user change details cannot be null");
        result.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetSystemUserAndOrganisation_ReturnsOk()
    {
        // Arrange
        var systemUser = new UserOrganisation
        {
            UserId = _userId,
            OrganisationId = _organisationId
        };

        var response = Result<UserOrganisation>.SuccessResult(systemUser);
        _userServiceMock.Setup(service => service.GetSystemUserAndOrganisationAsync(ApplicationUser.System))
            .ReturnsAsync(response);

        // Act
        var result = await _userController.GetSystemUserAndOrganisation() as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetSystemUserAndOrganisation_ReturnsNoContent()
    {
        // Arrange
        var response = Result<UserOrganisation>.FailedResult($"Error getting system user and Organisation", HttpStatusCode.NotFound);
        _userServiceMock.Setup(service => service.GetSystemUserAndOrganisationAsync(ApplicationUser.System))
            .ReturnsAsync(response);

        // Act
        var result = await _userController.GetSystemUserAndOrganisation() as NotFoundObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetSystemUserAndOrganisation_ReturnsException()
    {
        // Arrange
        _userServiceMock.Setup(service => service.GetSystemUserAndOrganisationAsync(ApplicationUser.System))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _userController.GetSystemUserAndOrganisation() as OkObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }
    
    [TestMethod]
    public async Task GetUserOrganisationsWithServiceRoles_ReturnsOk()
    {
        // Arrange
        var serviceKey = "Packaging";
        var expectedResponse = new UserOrganisationsListModel();
        var result = Result<UserOrganisationsListModel>.SuccessResult(expectedResponse);

        _userServiceMock
            .Setup(service => service.GetUserOrganisationsWithEnrolmentsAsync(_userId, serviceKey))
            .ReturnsAsync(result);

        // Act
        var response = await _userController.GetUserOrganisationsWithEnrolments(_userId, serviceKey) as OkObjectResult;

        // Assert
        response.Should().NotBeNull();
        response?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(expectedResponse);
    }
    
    [TestMethod]
    public async Task GetUserOrganisationsWithServiceRoles_ReturnsNoContent()
    {
        // Arrange
        var serviceKey = "Packaging";
        var result = Result<UserOrganisationsListModel>.FailedResult("User not found", HttpStatusCode.NoContent);

        _userServiceMock
            .Setup(service => service.GetUserOrganisationsWithEnrolmentsAsync(_userId, serviceKey))
            .ReturnsAsync(result);

        // Act
        var response = await _userController.GetUserOrganisationsWithEnrolments(_userId, serviceKey);

        // Assert
        var objectResult = response as ObjectResult;
        objectResult.Should().BeNull();
        objectResult?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task GetUserOrganisationsWithServiceRoles_ReturnsInternalServerError_OnException()
    {
        // Arrange
        var serviceKey = "Packaging";
        _userServiceMock
            .Setup(service => service.GetUserOrganisationsWithEnrolmentsAsync(_userId, serviceKey))
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var response = await _userController.GetUserOrganisationsWithEnrolments(_userId, serviceKey);

        // Assert
        var statusCodeResult = response as StatusCodeResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult?.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }
    
    [TestMethod]
    public async Task GetUserOrganisationsWithServiceRoles_WhenUserIdIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var emptyUserId = Guid.Empty;
    
        // Act
        var result = await _userController.GetUserOrganisationsWithEnrolments(emptyUserId, null);
    
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("The userId parameter is required.");
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_ReturnsOk_WithValidObjectId()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest
        {
            ObjectId = _userId
        };

        var OrganisationReferenceNumbers = new List<string> { "Org1", "Org2" };

        var response = Result<IList<string>>.SuccessResult(OrganisationReferenceNumbers);

        _userServiceMock.Setup(service => service.GetUserOrganisationIdListAsync(_userId))
            .ReturnsAsync(response);

        // Act
        var result = await _userController.GetUserOrganisationIds(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var responseValue = result?.Value as UserOrganisationIdentifiersResponse;
        responseValue.Should().NotBeNull();
        responseValue.OrganisationIds.Should().Be("Org1,Org2");
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_ReturnsOk_WithNoOrganisationIds_If_Db_Lookup_Fails()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest
        {
            ObjectId = _userId
        };

        var response = Result<IList<string>>.FailedResult("Not found", HttpStatusCode.NotFound);

        _userServiceMock.Setup(service => service.GetUserOrganisationIdListAsync(_userId))
            .ReturnsAsync(response);
                
        // Act
        var result = await _userController.GetUserOrganisationIds(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var responseValue = result?.Value as UserOrganisationIdentifiersResponse;
        responseValue.Should().NotBeNull();
        responseValue.OrganisationIds.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetUserOrganisationIds_ReturnsInternalServerError_OnException()
    {
        // Arrange
        var request = new UserOrganisationIdentifiersRequest
        {
            ObjectId = _userId
        };

        _userServiceMock.Setup(service => service.GetUserOrganisationIdListAsync(_userId))
            .ThrowsAsync(new Exception("Simulated error"));

        // Act
        var result = await _userController.GetUserOrganisationIds(request) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }
}