using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers.ConnectionControllerTests;

[TestClass]
public class ConnectionsControllerTests
{
    private readonly Mock<IRoleManagementService> _roleManagementServiceMock = new();
    private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    private readonly NullLogger<ConnectionsController> _nullLogger = new();
    private readonly Mock<IValidationService> _validationServiceMock = new();
    private readonly Guid _connectionExternalId = Guid.NewGuid();
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
            .Setup(x => x.IsAuthorisedToManageUsersFromOrganisationForService(
                _userId, _organisationId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
            .ReturnsAsync(false);

        var result = await _connectionsController.GetConnectionAndEnrolments(_connectionExternalId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, _userId, _organisationId) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problemDetails = result.Value as ProblemDetails;

        problemDetails.Type.Should().Be("https://epr-errors/authorisation");
    }

    [TestMethod]
    public async Task WhenAuthorisedUserHasNoConnection_ThenReturnStatus404NotFound()
    {
        _validationServiceMock
            .Setup(x => x.IsAuthorisedToManageUsersFromOrganisationForService(
                _userId, _organisationId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
            .ReturnsAsync(true);

        _roleManagementServiceMock
            .Setup(x => x.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(
                _connectionExternalId, _organisationId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
            .ReturnsAsync(null as ConnectionWithEnrolmentsResponse);

        var result = await _connectionsController.GetConnectionAndEnrolments(_connectionExternalId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, _userId, _organisationId) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problemDetails = result.Value as ProblemDetails;

        problemDetails.Type.Should().Be("https://epr-errors/connection-not-found");
    }

    [TestMethod]
    public async Task WhenAuthorisedUserHasConnection_ThenReturnsStatusOkAndData()
    {
        _validationServiceMock
            .Setup(x => x.IsAuthorisedToManageUsersFromOrganisationForService(
                _userId, _organisationId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
            .ReturnsAsync(true);

        _roleManagementServiceMock
            .Setup(x => x.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(
                _connectionExternalId, _organisationId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
            .ReturnsAsync(new ConnectionWithEnrolmentsResponse
            {
                PersonRole = RoleManagementService.MapPersonRole(Data.DbConstants.PersonRole.Admin),
                Enrolments = new List<EnrolmentsFromConnectionResponse>
                {
                    new()
                    {
                        EnrolmentStatus = RoleManagementService.MapEnrolmentStatus(Data.DbConstants.EnrolmentStatus.Approved),
                        ServiceRoleKey = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key
                    }
                }
            });

        var result = await _connectionsController.GetConnectionAndEnrolments(_connectionExternalId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, _userId, _organisationId) as ObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);

        var resultValue = result.Value as ConnectionWithEnrolmentsResponse;

        resultValue.Should().NotBeNull();
        resultValue.Enrolments.Should().HaveCount(1);
        resultValue.Enrolments.First().EnrolmentStatus.Should().Be(EnrolmentStatus.Approved);
        resultValue.PersonRole.Should().Be(PersonRole.Admin);
    }
}
