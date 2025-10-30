using BackendAccountService.Core.Models.Request;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Services;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers
{
    [ApiController]
    [Route("api/enrolments")]
    public class ApprovedPersonEnrolmentsController : ApiControllerBase
    {
        private readonly IRoleManagementService _roleManagementService;
        private readonly ILogger<ApprovedPersonEnrolmentsController> _logger;

        public ApprovedPersonEnrolmentsController(
            IRoleManagementService roleManagementService,
            IOptions<ApiConfig> baseApiConfigOptions,
            ILogger<ApprovedPersonEnrolmentsController> logger) : base(baseApiConfigOptions)
        {
            _roleManagementService = roleManagementService;
            _logger = logger;
        }

        [HttpPut]
        [Consumes("application/json")]
        [Route("{enrolmentId:guid}/approved-person-acceptance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcceptNominationForApprovedPerson(
            Guid enrolmentId,
            [BindRequired, FromQuery] string serviceKey,
            [BindRequired, FromBody] AcceptNominationForApprovedPersonRequest acceptNominationRequest,
            [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
            [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
        {
            if (serviceKey != "Packaging")
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, type: "service-not-supported");
            }

            var result = await _roleManagementService.AcceptNominationForApprovedPerson(enrolmentId, userId, organisationId, serviceKey, acceptNominationRequest);

            if (result.Succeeded)
            {
                return Ok();
            }

            _logger.LogError(
                "User {UserId} from organisation {OrganisationId} and service '{ServiceKey}' failed to accept nomination to become an Approved Person (enrolment: {EnrolmentId}. Error: {ErrorMessage}",
                userId, organisationId, serviceKey, enrolmentId, result.ErrorMessage);

            return Problem(
                title: "Failed to accept the nomination to Approved Person",
                statusCode: StatusCodes.Status400BadRequest,
                type: "approved-person-nomination",
                detail: result.ErrorMessage);
        }       
    }
}
