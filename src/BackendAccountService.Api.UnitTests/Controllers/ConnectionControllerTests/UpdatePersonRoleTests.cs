using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Exceptions;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace BackendAccountService.Api.UnitTests.Controllers.ConnectionControllerTests
{
    [TestClass]
    public class UpdatePersonRoleTests
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
                .Setup(x => x.IsAuthorisedToManageUsersFromOrganisationForService(
                    _userId, _organisationId, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
                .ReturnsAsync(false);

            var updateRequest = new UpdatePersonRoleRequest
            {
                PersonRole = Core.Models.PersonRole.Admin
            };

            var result = await _connectionsController.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _userId, _organisationId) as ObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

            var problemDetails = result.Value as ProblemDetails;

            problemDetails.Type.Should().Be("https://epr-errors/authorisation");
        }

        [TestMethod]
        public async Task WhenPersonRoleUpdateIsSuccessful_ThenReturnOk()
        {
            _validationServiceMock
                .Setup(x => x.IsAuthorisedToManageUsersFromOrganisationForService(_userId, _organisationId, "Packaging"))
                .ReturnsAsync(true);

            _roleManagementServiceMock
                .Setup(x => x.UpdatePersonRoleAsync(_connectionId, _userId, _organisationId, "Packaging", Core.Models.PersonRole.Admin))
                .ReturnsAsync(new UpdatePersonRoleResponse());

            var updateRequest = new UpdatePersonRoleRequest
            {
                PersonRole = Core.Models.PersonRole.Admin
            };

            var result = await _connectionsController.UpdatePersonRole(_connectionId,  updateRequest, "Packaging", _userId, _organisationId) as OkObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task WhenPersonRoleUpdateIsNotSuccessful_ThenReturnBadRequestProblem()
        {
            _validationServiceMock
                .Setup(x => x.IsAuthorisedToManageUsersFromOrganisationForService(_userId, _organisationId, "Packaging"))
                .ReturnsAsync(true);

            _roleManagementServiceMock
                .Setup(x => x.UpdatePersonRoleAsync(_connectionId, _userId, _organisationId, "Packaging", Core.Models.PersonRole.Admin))
                .Throws(new RoleManagementException("Only approved person can edit delegated person enrolment"));

            var updateRequest = new UpdatePersonRoleRequest
            {
                PersonRole = Core.Models.PersonRole.Admin
            };

            var result = await _connectionsController.UpdatePersonRole(_connectionId, updateRequest, "Packaging", _userId, _organisationId) as ObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var problemDetails = result.Value as ProblemDetails;

            problemDetails.Type.Should().Be("https://epr-errors/person-role");
            problemDetails.Detail.Should().Be("Only approved person can edit delegated person enrolment");
        }
    }
}
