using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/regulators")]
public class RegulatorsController : ApiControllerBase
{
    private readonly IRegulatorService _regulatorService;
    private readonly IOrganisationService _organisationService;
    private readonly IValidationService _validationService;
    private readonly ILogger<RegulatorsController> _logger;

    public RegulatorsController(
        IRegulatorService regulatorService,
        IOrganisationService organisationService,
        IValidationService validationService,
        ILogger<RegulatorsController> logger,
        IOptions<ApiConfig> baseApiConfigOptions)
        : base(baseApiConfigOptions)
    {
        _regulatorService = regulatorService;
        _organisationService = organisationService;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpGet]
    [Route("pending-applications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingApplications(Guid userId, int currentPage, int pageSize, string? organisationName, string? applicationType)
    {
        var isUserAuthorised = _regulatorService.IsRegulator(userId);

        if (!isUserAuthorised)
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }
        int nationId = _regulatorService.GetRegulatorNationId(userId);
        if (nationId == 0 || currentPage < 1 || pageSize < 1)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await _regulatorService.GetPendingApplicationsAsync(nationId, currentPage, pageSize,
            organisationName, applicationType ?? "All");

        var lastPage = (int) Math.Ceiling((double) result.TotalItems / result.PageSize);
        if (currentPage > lastPage && lastPage > 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest);
        }
        return Ok(result);
    }


    [HttpGet]
    [Route("applications/enrolments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingApplicationsForOrganisation(Guid userId, Guid organisationId)
    {
        return await ExecuteProtectedAction(userId, organisationId, async () =>
        {
            var result = await _regulatorService.GetOrganisationEnrolmentDetails(organisationId);

            return Ok(result);
        });
    }
    
    [HttpGet]
    [Consumes("application/json")]
    [Route("organisations/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Users(Guid userId, Guid organisationId, bool getApprovedUsersOnly)
    {
        return await ExecuteProtectedAction(userId, organisationId, async () =>
        {
            var result = await _regulatorService.GetUserListForRegulator(organisationId, getApprovedUsersOnly);
            
            return new OkObjectResult(result);
        });
    }
    
    [HttpPost]
    [Route("accounts/manage-enrolment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEnrolment([FromBody]ManageRegulatorEnrolmentRequest request)
    {
        var organisationId = await _organisationService.GetOrganisationIdFromEnrolment(request.EnrolmentId);
        return await ExecuteProtectedAction(request.UserId, organisationId, async () =>
        {
            var result = await _regulatorService.UpdateEnrolmentStatusForUserAsync(
                request.UserId,
                organisationId,
                request.EnrolmentId,
                request.EnrolmentStatus,
                request.RegulatorComment);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "regulator user {UserId} failed to update enrolment '{enrolmentId}: {Message}'", 
                    request.UserId, request.EnrolmentId, result.ErrorMessage);
            
                return Problem(
                    title: "Failed to update enrolment",
                    statusCode: StatusCodes.Status400BadRequest, 
                    type: "enrolment-status",
                    detail: result.ErrorMessage);
            }

            return NoContent();
        });
    }

    [HttpPost]
    [Route("organisation/transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransferOrganisationNation(OrganisationTransferNationRequest request)
    {
        return await ExecuteProtectedAction(request.UserId, request.OrganisationId, async () =>
        {
            if (request.TransferNationId == 0)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, type: "Invalid Nation");
            }

            var result = await _regulatorService.TransferOrganisationNation(request);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "regulator user {UserId} failed to transfer organisation '{organisation}: {Message}'",
                    request.UserId, request.OrganisationId, result.ErrorMessage);

                return Problem(
                    title: "Failed to update enrolment",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "enrolment-status",
                    detail: result.ErrorMessage);
            }

            return NoContent();

        });
    }
    
    [HttpGet]
    [Route("get-organisations-by-search-term")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationsBySearchTerm(Guid userId, int currentPage, int pageSize, string query)
    {
        var isUserAuthorised = _regulatorService.IsRegulator(userId);
        if (!isUserAuthorised)
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }
        
        var nationId = _regulatorService.GetRegulatorNationId(userId);
        if (nationId == 0 || currentPage < 1 || pageSize < 1)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest);
        }
        
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, nationId, pageSize, currentPage);
        return Ok(results);
    }
    
    [HttpGet]
    [Route("organisation-by-external-id")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationsByOrganisationId(Guid organisationId, Guid userId)
    {
        return await ExecuteProtectedAction(userId, organisationId,async () =>
        {
            var result = await _regulatorService.GetCompanyDetailsById(organisationId);

            return Ok(result);
        });
    }
    
    [HttpGet]
    [Route("users-by-organisation-external-id")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersByOrganisationExternalId(Guid userId, Guid externalId)
    {
        return await ExecuteProtectedAction(userId, externalId,async () =>
        {
            var producerUsers = await _organisationService.GetProducerUsers(externalId);
            return Ok(producerUsers);
        });
    }


    [HttpPost]
    [Route("remove-approved-users")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveApprovedPerson(RemoveApprovedUserRequest request)
    {
        return await ExecuteProtectedAction(request.UserId, request.OrganisationId,async () =>
        {
            if (request.ConnectionExternalId == Guid.Empty )
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, type: "Invalid Data");
            }

            var associatedPerson =  await _regulatorService.RemoveApprovedPerson(request);

            return Ok(associatedPerson);
        });
    }
    
    [HttpPost]
    [Route("add-remove-approved-users")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> AddRemoveApprovedUser(AddRemoveApprovedUserRequest request)
    {
        return await ExecuteProtectedAction(request.AddingOrRemovingUserId, request.OrganisationId,async () =>
        {
            if (string.IsNullOrWhiteSpace(request.AddingOrRemovingUserEmail)
                || string.IsNullOrWhiteSpace(request.InvitedPersonEmail))
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, type: "Invalid Data");
            }
            
            var isUserInvited = await _validationService.IsUserInvitedAsync(request.InvitedPersonEmail);

            if (isUserInvited)
            {
                var userAlreadyInvitedError = $"User '{request.InvitedPersonEmail}' is already invited";
                ModelState.AddModelError(nameof(request.InvitedPersonEmail), userAlreadyInvitedError);
                return BadRequest(userAlreadyInvitedError);
            }

            var response = await _regulatorService.AddRemoveApprovedPerson(request);
            return Ok(response);
        });
    }
    
    private async Task<IActionResult> ExecuteProtectedAction(Guid userId, Guid organisationId, Func<Task<IActionResult>> action) 
    {
        if (userId == Guid.Empty || organisationId == Guid.Empty)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, type: "Invalid Data");
        }
        
        var isUserAuthorised = _regulatorService.DoesRegulatorNationMatchOrganisationNation(userId, organisationId);
        if (!isUserAuthorised)
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }
        return await action.Invoke();
    }
}