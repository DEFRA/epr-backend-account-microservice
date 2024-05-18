
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrganisationType = BackendAccountService.Core.Models.OrganisationType;
using ProducerType = BackendAccountService.Core.Models.ProducerType;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class ApprovedPersonEnrolmentsControllerTests
{
    private ApprovedPersonEnrolmentsController _controller = null!;
    private Mock<IRoleManagementService> _roleManagementServiceMock = null!;
    private Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = null!;
    private Mock<ILogger<ApprovedPersonEnrolmentsController>> _logger = new();

    [TestInitialize]
    public void Setup()
    {
        _roleManagementServiceMock = new Mock<IRoleManagementService>();

        _apiConfigOptionsMock = new Mock<IOptions<ApiConfig>>();

        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

        _controller = new ApprovedPersonEnrolmentsController(_roleManagementServiceMock.Object,
            _apiConfigOptionsMock.Object,
            _logger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task AcceptNominationForApprovedPerson_WhenValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        (bool Succeeded, string ErrorMessage) acceptNominationForApprovedPersonResult = (true, string.Empty);

        _roleManagementServiceMock.Setup(x => x.AcceptNominationForApprovedPerson(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), 
            It.IsAny<AcceptNominationForApprovedPersonRequest>())).ReturnsAsync(acceptNominationForApprovedPersonResult);

        //  Act
        var result = await _controller.AcceptNominationForApprovedPerson(Guid.NewGuid(), "Packaging", new AcceptNominationForApprovedPersonRequest(),
            Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeOfType<OkResult>();
    }
    
    [TestMethod]
    public async Task AcceptNominationForApprovedPerson_WhenInValidRequest_ShouldReturnBadRequestStatus()
    {
        // Arrange
        (bool Succeeded, string ErrorMessage) acceptNominationForApprovedPersonResult = (false, string.Empty);

        _roleManagementServiceMock.Setup(x => x.AcceptNominationForApprovedPerson(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
            It.IsAny<AcceptNominationForApprovedPersonRequest>())).ReturnsAsync(acceptNominationForApprovedPersonResult);

        //  Act
        var result = await _controller.AcceptNominationForApprovedPerson(Guid.NewGuid(), "Packaging", new AcceptNominationForApprovedPersonRequest(),
            Guid.NewGuid(), Guid.NewGuid()) as ObjectResult;

        // Assert
        var problemDetails = result?.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails?.Type.Should().Be("https://dummytest/approved-person-nomination");
        problemDetails?.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task AcceptNominationForApprovedPerson_WhenServiceKeyIsInvalid_ReturnProblem()
    {
        // Arrange
        var serviceKey = "Not packaging";

        //  Act
        var result = await _controller.AcceptNominationForApprovedPerson(Guid.NewGuid(), serviceKey, new AcceptNominationForApprovedPersonRequest(),
            Guid.NewGuid(), Guid.NewGuid()) as ObjectResult;

        // Assert
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);        
    }
}