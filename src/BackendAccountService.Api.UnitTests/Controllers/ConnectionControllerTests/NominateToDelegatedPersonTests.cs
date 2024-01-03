using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers.ConnectionControllerTests;

[TestClass]
[TestCategory("Nominating Delegated Person")]
public class NominateToDelegatedPersonTests
{
    private readonly Mock<IRoleManagementService> _roleManagementServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<ConnectionsController> _nullLogger = new();
    private readonly Mock<IValidationService> _validationServiceMock = new();
    private readonly Guid _connectionId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private ConnectionsController _connectionsController = null!;

    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock.Setup(x => x.Value)
            .Returns(new ApiConfig { BaseProblemTypePath = "https://epr-errors/" });

        _connectionsController = new ConnectionsController(
            _validationServiceMock.Object,
            _roleManagementServiceMock.Object,
            _apiConfigOptionsMock.Object,
            _nullLogger)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [TestMethod]
    public async Task WhenUserIsNotAuthorised_ThenReturnStatus403ForbiddenProblem()
    {
        _validationServiceMock
            .Setup(x => x.IsAuthorisedToManageDelegatedUsersFromOrganisationForService(
                _userId, _organisationId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
            .ReturnsAsync(false);

        var result = await _connectionsController.NominateToDelegatedPerson(
            connectionId: _connectionId,
            serviceKey: "Packaging",
            userId: _userId,
            organisationId: _organisationId,
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            }) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problemDetails = result.Value as ProblemDetails;

        problemDetails.Type.Should().Be("https://epr-errors/authorisation");
    }

    [TestMethod]
    public async Task WhenNominateToDelegatedPersonIsSuccessful_ThenReturnOk()
    {
        _validationServiceMock
            .Setup(x => x.IsAuthorisedToManageDelegatedUsersFromOrganisationForService(_userId, _organisationId, "Packaging"))
            .ReturnsAsync(true);

        var nominationRequest = new Core.Models.Request.DelegatedPersonNominationRequest
        {
            ConsultancyName = "Test Consultancy",
            NominatorDeclaration = "Test Tester"
        };

        _roleManagementServiceMock
            .Setup(x => x.NominateToDelegatedPerson(_connectionId, _userId, _organisationId, "Packaging", nominationRequest))
            .ReturnsAsync((true, string.Empty));

        var result = await _connectionsController.NominateToDelegatedPerson(
            connectionId: _connectionId,
            serviceKey: "Packaging",
            userId: _userId,
            organisationId: _organisationId,
            nominationRequest: nominationRequest) as OkResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [TestMethod]
    public async Task WhenNominateToDelegatedPersonIsNotSuccessful_ThenReturnBadRequestProblem()
    {
        _validationServiceMock
            .Setup(x => x.IsAuthorisedToManageDelegatedUsersFromOrganisationForService(_userId, _organisationId, "Packaging"))
            .ReturnsAsync(true);

        var nominationRequest = new Core.Models.Request.DelegatedPersonNominationRequest
        {
            ConsultancyName = "Test Consultancy",
            NominatorDeclaration = "Test Tester"
        };

        _roleManagementServiceMock
            .Setup(x => x.NominateToDelegatedPerson(_connectionId, _userId, _organisationId, "Packaging", nominationRequest))
            .ReturnsAsync((false, "Nominating delegated person failed"));

        var result = await _connectionsController.NominateToDelegatedPerson(
            connectionId: _connectionId,
            serviceKey: "Packaging",
            userId: _userId,
            organisationId: _organisationId,
            nominationRequest: nominationRequest) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = result.Value as ProblemDetails;

        problemDetails.Type.Should().Be("https://epr-errors/delegated-person-invitation");
        problemDetails.Detail.Should().Be("Nominating delegated person failed");
    }
}