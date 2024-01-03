using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class EnrolmentControllerTests
{
    private readonly Mock<IEnrolmentsService> _enrolmentServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<EnrolmentsController> _nullLogger = new();
    private readonly Mock<IValidationService> _validationServiceMock = new();
    private readonly Guid _loggedInUserId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly string _baseProblemTypePath = "https://dummytest/";
    private EnrolmentsController enrolmentController = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = _baseProblemTypePath
            });
        
        enrolmentController = new EnrolmentsController(
            _enrolmentServiceMock.Object, 
            _apiConfigOptionsMock.Object, 
            _nullLogger,
            _validationServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task RemoveEnrolment_WhenUserIsNotAuthorised_Returns403()
    {
        // Arrange
        _validationServiceMock
            .Setup(x => x.IsAuthorisedToRemoveEnrolledUser(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(false);
        
        // Act
        var result = await enrolmentController.RemoveEnrolment(_personId, _loggedInUserId, _organisationId, 1);
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(StatusCodes.Status403Forbidden, ((ObjectResult)result).StatusCode);
        var problemDetails = (result as ObjectResult).Value as ProblemDetails;
        Assert.AreEqual($"{_baseProblemTypePath}authorisation", problemDetails.Type);
    }

    [TestMethod]
    public async Task RemoveEnrolment_WhenUserIsAuthorised_AndRemoveSucceeds_ShouldReturnNoContent()
    {
        // Arrange
        _validationServiceMock
            .Setup(x => x.IsAuthorisedToRemoveEnrolledUser(_loggedInUserId, _organisationId, 1, _personId))
            .Returns(true);
        
        _enrolmentServiceMock.Setup(x => x.DeleteEnrolmentsForPersonAsync(_loggedInUserId, _personId, _organisationId, 1))
            .ReturnsAsync(true);
        
        // Act
        var result = await enrolmentController.RemoveEnrolment(_personId, _loggedInUserId, _organisationId, 1);
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task RemoveEnrolment_WhenUserIsAuthorised_AndRemoveFails_ShouldReturnProblem()
    {
        // Arrange
        _validationServiceMock
            .Setup(x => x.IsAuthorisedToRemoveEnrolledUser(_loggedInUserId, _organisationId, 1, _personId))
            .Returns(true);
        
        _enrolmentServiceMock.Setup(x => x.DeleteEnrolmentsForPersonAsync(_loggedInUserId, _personId, _organisationId, 1))
            .ReturnsAsync(false);
        
        // Act
        var result = await enrolmentController.RemoveEnrolment(_personId, _loggedInUserId, _organisationId, 1);
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
        var problemDetails = (result as ObjectResult).Value as ProblemDetails;
        Assert.AreEqual($"{_baseProblemTypePath}failed to remove person", problemDetails.Type);
    }
}