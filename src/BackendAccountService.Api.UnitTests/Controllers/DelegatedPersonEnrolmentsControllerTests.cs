using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class DelegatedPersonEnrolmentsControllerTests
{
    private DelegatedPersonEnrolmentsController _delegatedPersonEnrolmentsController = null!;
    private readonly Mock<IRoleManagementService> _roleManagementServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<DelegatedPersonEnrolmentsController> _nullLogger = new();

    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _delegatedPersonEnrolmentsController = new DelegatedPersonEnrolmentsController(_roleManagementServiceMock.Object, _apiConfigOptionsMock.Object, _nullLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    [TestCategory("AcceptNominationToDelegatedPerson")]
    public async Task AcceptNominationToDelegatedPerson_WhenServiceKeyNotPackaging_ThenAcceptingNominationFails()
    {
        // Arrange
        var enrolmentId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        // Act
        var result = await _delegatedPersonEnrolmentsController.AcceptNominationToDelegatedPerson(
            enrolmentId,
            "SomethingOtherThanPackaging",
            new Core.Models.Request.AcceptNominationRequest(),
            Guid.NewGuid(),
            organisationId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://dummytest/service-not-supported");
        problem.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [TestMethod]
    [TestCategory("AcceptNominationToDelegatedPerson")]
    public async Task AcceptNominationToDelegatedPerson_WhenNominationAccepted_ThenReturnsOkResult()
    {
        // Arrange
        const string serviceKey = "Packaging";
        var enrolmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var request = new Core.Models.Request.AcceptNominationRequest();

        _roleManagementServiceMock.Setup(service => service.AcceptNominationToDelegatedPerson(enrolmentId, userId, organisationId, serviceKey, request))
            .ReturnsAsync(new ValueTuple<bool, string>(true, string.Empty));

        // Act
        var result = await _delegatedPersonEnrolmentsController.AcceptNominationToDelegatedPerson(
            enrolmentId,
            serviceKey,
            request,
            userId,
            organisationId);

        // Assert
        result.Should().BeOfType<OkResult>();
        _roleManagementServiceMock.Verify(service => service.AcceptNominationToDelegatedPerson(enrolmentId, userId, organisationId, serviceKey, request), Times.Once);
    }

    [TestMethod]
    [TestCategory("AcceptNominationToDelegatedPerson")]
    public async Task AcceptNominationToDelegatedPerson_WhenNominationNotAccepted_ThenReturnsBadRequestResult()
    {
        // Arrange
        const string serviceKey = "Packaging";
        var enrolmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var request = new Core.Models.Request.AcceptNominationRequest();

        _roleManagementServiceMock.Setup(service => service.AcceptNominationToDelegatedPerson(enrolmentId, userId, organisationId, serviceKey, request))
            .ReturnsAsync(new ValueTuple<bool, string>(false, "SomeErrorMessage"));

        // Act
        var result = await _delegatedPersonEnrolmentsController.AcceptNominationToDelegatedPerson(
            enrolmentId,
            serviceKey,
            request,
            userId,
            organisationId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://dummytest/delegated-person-nomination");
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [TestMethod]
    [TestCategory("GetDelegatedPersonNominator")]
    public async Task GetDelegatedPersonNominator_WhenNominationNotFound_ThenReturnsNotFoundResult()
    {
        // Arrange
        const string serviceKey = "Packaging";
        var enrolmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        _roleManagementServiceMock.Setup(service => service.GetDelegatedPersonNominator(enrolmentId, userId, organisationId, serviceKey))
            .ReturnsAsync((DelegatedPersonNominatorResponse)null!);

        // Act
        var result = await _delegatedPersonEnrolmentsController.GetDelegatedPersonNominator(
            enrolmentId,
            userId,
            organisationId,
            serviceKey) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problem = result.Value as ProblemDetails;

        problem.Should().NotBeNull();
        problem.Type.Should().Be("https://dummytest/delegated-person-nominator-not-found");
        problem.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [TestMethod]
    [TestCategory("GetDelegatedPersonNominator")]
    public async Task GetDelegatedPersonNominator_WhenNominationAccepted_ThenReturnsSuccessfully()
    {
        // Arrange
        const string serviceKey = "Packaging";
        var enrolmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        const string expectedFirstName = "TestFirstName";
        const string expectedLastName = "TestFirstName";
        const string expectedOrganisationName = "TestOrganisationName";

        _roleManagementServiceMock.Setup(service => service.GetDelegatedPersonNominator(enrolmentId, userId, organisationId, serviceKey))
            .ReturnsAsync(new DelegatedPersonNominatorResponse { FirstName = expectedFirstName, LastName = expectedLastName, OrganisationName = expectedOrganisationName });

        // Act
        var result = await _delegatedPersonEnrolmentsController.GetDelegatedPersonNominator(
            enrolmentId,
            userId,
            organisationId,
            serviceKey);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var response = ((result as OkObjectResult)!).Value as DelegatedPersonNominatorResponse;
        response.FirstName.Should().Be(expectedFirstName);
        response.LastName.Should().Be(expectedLastName);
        response.OrganisationName.Should().Be(expectedOrganisationName);
    }
}