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
    public class DelegatedPersonEnrolmentsController : ApiControllerBase
    {
        private readonly IRoleManagementService _roleManagementService;
        private readonly ILogger<DelegatedPersonEnrolmentsController> _logger;

        public DelegatedPersonEnrolmentsController(
            IRoleManagementService roleManagementService,
            IOptions<ApiConfig> baseApiConfigOptions,
            ILogger<DelegatedPersonEnrolmentsController> logger) : base(baseApiConfigOptions)
        {
            _roleManagementService = roleManagementService;
            _logger = logger;
        }

        [HttpPut]
        [Consumes("application/json")]
        [Route("{enrolmentId:guid}/delegated-person-acceptance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptNominationToDelegatedPerson(
            Guid enrolmentId,
            [BindRequired, FromQuery] string serviceKey,
            [BindRequired, FromBody] AcceptNominationRequest acceptNominationRequest,
            [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
            [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
        {
            if (serviceKey != "Packaging")
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, type: "service-not-supported");
            }

            var result = await _roleManagementService.AcceptNominationToDelegatedPerson(enrolmentId, userId, organisationId, serviceKey, acceptNominationRequest);

            if (result.Succeeded)
            {
                return Ok();
            }

            _logger.LogError(
                "User {UserId} from organisation {OrganisationId} and service '{ServiceKey}' failed to accept nomination to Delegated Person (enrolment: {EnrolmentId}. Error: {ErrorMessage}",
                userId, organisationId, serviceKey, enrolmentId, result.ErrorMessage);

            return Problem(
                title: "Failed to accept the nomination to Delegated Person",
                statusCode: StatusCodes.Status400BadRequest,
                type: "delegated-person-nomination",
                detail: result.ErrorMessage);
        }
        
        
        [HttpGet]
        [Route("{enrolmentId:guid}/delegated-person-nominator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetDelegatedPersonNominator(
            Guid enrolmentId,
            [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
            [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId,
            [BindRequired, FromQuery] string serviceKey)
        {
            var response = await _roleManagementService.GetDelegatedPersonNominator(enrolmentId, userId, organisationId, serviceKey);
            if (response == null)
            {
                _logger.LogError("Delegated person nominator for enrolment {enrolmentId} could not be found", enrolmentId);
                return Problem(statusCode: StatusCodes.Status404NotFound, type: "delegated-person-nominator-not-found");
            }
        
            return new OkObjectResult(response);
        }
    }
}
