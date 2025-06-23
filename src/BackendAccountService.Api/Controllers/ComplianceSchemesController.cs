using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Helpers;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/compliance-schemes")]
public class ComplianceSchemesController : ApiControllerBase
{
    private readonly IComplianceSchemeService _complianceSchemeService;
    private readonly ILogger<ComplianceSchemesController> _logger;
    private readonly IValidationService _validationService;
    
    public ComplianceSchemesController(
        IComplianceSchemeService complianceSchemeService, 
        IOptions<ApiConfig> baseApiConfigOptions,
        ILogger<ComplianceSchemesController> logger,
        IValidationService validateDataService)
        : base(baseApiConfigOptions)
    {
        _complianceSchemeService = complianceSchemeService;
        _logger = logger;
        _validationService = validateDataService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ComplianceSchemeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllComplianceSchemes()
    {
        var result = await _complianceSchemeService.GetAllComplianceSchemesAsync();

        return Ok(result);
    }

    [Route("{organisationId:guid}/schemes/{complianceSchemeId:guid}/scheme-members")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ComplianceSchemeMembershipResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComplianceSchemeMembers(
        Guid organisationId,
        Guid complianceSchemeId,
        [BindRequired, FromQuery] int pageSize,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
        string? query,
        int page = 1 )
    {
        return await ExecuteProtectedComplianceSchemeAction(userId, organisationId, async () =>
        {
            _logger.LogInformation($"Filtering the members for the compliance scheme id {organisationId}");

            var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(organisationId, complianceSchemeId, query, pageSize, page);
            if (!result.IsSuccess)
            {
                _logger.LogInformation("Error getting members for the compliance scheme id {complianceSchemeId}", complianceSchemeId);
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("Members fetched successfully for the compliance scheme id {complianceSchemeId}", complianceSchemeId);
            return Ok(result.Value);
        });
    }

    [HttpPost]
    [Route("remove")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveComplianceScheme(RemoveComplianceSchemeRequest removeComplianceScheme)
    {
        return await ExecuteProtectedAction(removeComplianceScheme.UserOId, removeComplianceScheme.OrganisationId, async () =>
        {
            _logger.LogInformation($"Removing the selected scheme id {removeComplianceScheme.SelectedSchemeId}");

            var result = await _complianceSchemeService.RemoveComplianceSchemeAsync(removeComplianceScheme);

            if (!result.IsSuccess)
            {
                _logger.LogInformation("Error removing the selected compliance scheme id {SchemeId}", removeComplianceScheme.SelectedSchemeId);
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("Removed the selected compliance scheme id {SchemeId}", removeComplianceScheme.SelectedSchemeId);
            return Ok();
        });
    }
    
    [Route("select")]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SelectedSchemeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SelectComplianceScheme(SelectComplianceSchemeRequest request)
    {
        return await ExecuteProtectedAction(request.UserOId, request.ProducerOrganisationId, async () =>
        { 
            var result = await _complianceSchemeService.SelectComplianceSchemeAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to select compliance scheme {ModelComplianceSchemeId} for user {ModelUserOid}", 
                    request.ComplianceSchemeId, request.UserOId);
                return result.BuildErrorResponse();
            }

            return Ok(new SelectedSchemeDto(result.Value.ExternalId));
        });
    }

    [Route("update")]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SelectedSchemeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSelectedComplianceScheme(UpdateSelectedComplianceSchemeRequest request)
    {
        return await ExecuteProtectedAction(request.UserOid, request.ProducerOrganisationId, async () => 
        {
            var result = await _complianceSchemeService.UpdateSelectedComplianceSchemeAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to select compliance scheme {ModelComplianceSchemeId} for user {ModelUserOid}", 
                    request.ComplianceSchemeId, request.UserOid);    
                return result.BuildErrorResponse();
            }

            return Ok(new SelectedSchemeDto(result.Value.ExternalId));
        });
    }

    [Route("get-for-producer")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProducerComplianceSchemeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetComplianceSchemeForProducer(Guid userOid, Guid organisationId)
    {
        _logger.LogInformation($"Fetching the selected scheme for the organsiation id {organisationId}");

        var result = await _complianceSchemeService.GetComplianceSchemeForProducer(organisationId);

        if (!result.IsSuccess)
        {
            _logger.LogInformation("error getting selected scheme for the organisation id {organisationId}",
                organisationId);
            return result.BuildErrorResponse();
        }

        _logger.LogInformation("selected scheme fetched successfully for the organisation id {organisationId}",
            organisationId);
        return new OkObjectResult(result.Value);
    }

    [Route("get-for-operator")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceSchemeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetComplianceSchemesForOperator(Guid organisationId)
    {
        var result = await _complianceSchemeService.GetComplianceSchemesForOperatorAsync(organisationId);

        return Ok(result);
    }
    
    [Route("{organisationId:guid}/scheme-members/{selectedSchemeId:guid}")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ComplianceSchemeMemberDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComplianceSchemeMemberDetails(
        Guid organisationId,
        Guid selectedSchemeId,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId)
    {
        return await ExecuteProtectedComplianceSchemeAction(userId, organisationId, async () =>
        {

            var result = await _complianceSchemeService.GetComplianceSchemeMemberDetailsAsync(organisationId, selectedSchemeId);

            if (!result.IsSuccess)
            {
                _logger.LogInformation("Error getting member details for the compliance scheme organisation id {organisationId} and selected scheme id {selectedSchemeId}", organisationId, selectedSchemeId);
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("Members fetched successfully for the compliance scheme organisation id {organisationId} and selected scheme id {selectedSchemeId}", organisationId, selectedSchemeId);
            return Ok(result.Value);
        });
    }

    [Route("{organisationId:guid}/scheme-members/{selectedSchemeId:guid}/removal")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(InfoForSelectedSchemeRemoval), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveComplianceSchemeMember(
        [BindRequired, FromRoute] Guid organisationId,
        [BindRequired, FromRoute] Guid selectedSchemeId,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId)
    {
        return await ExecuteProtectedComplianceSchemeAction(userId, organisationId, async () =>
        {
            var result = await _complianceSchemeService.GetInfoForSelectedSchemeRemoval(organisationId, selectedSchemeId, userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            _logger.LogInformation("Error getting selected scheme removal info for {SelectedSchemeId}", selectedSchemeId);

            return result.BuildErrorResponse();
        });
    }    
    
    [Route("{organisationId:guid}/scheme-members/{selectedSchemeId:guid}/removed")]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RemoveComplianceSchemeMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveComplianceSchemeMember(
        [BindRequired, FromRoute] Guid organisationId,
        [BindRequired, FromRoute] Guid selectedSchemeId,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
        [BindRequired, FromBody] RemoveComplianceSchemeMemberRequest request)
    {
        return await ExecuteProtectedComplianceSchemeAction(userId, organisationId, async () =>
        {
            var result = await _complianceSchemeService.RemoveComplianceSchemeMember(organisationId, selectedSchemeId, userId, request);
            if (!result.IsSuccess)
            {
                _logger.LogInformation("Error removing the selected scheme id {selectedSchemeId}", selectedSchemeId);
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("Removed the selected scheme id {selectedSchemeId}", selectedSchemeId);
            
            return Ok(result.Value);
        });
    }

    [Route("{complianceSchemeId:guid}/summary")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(List<ComplianceSchemeSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComplianceSchemesSummary(
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId,
        Guid complianceSchemeId)
    {
        return await ExecuteProtectedComplianceSchemeAction(userId, organisationId, async () =>
        {
            var result = await _complianceSchemeService.GetComplianceSchemeSummary(organisationId, complianceSchemeId);

            if (result != null)
            {
                return Ok(result);
            }

            _logger.LogInformation(
                "Failed to get compliance scheme {ComplianceSchemeId} summary for user {UserId} in organisation {OrganisationId} ",
                complianceSchemeId, userId, organisationId);                    
                    
            return NotFound();
        });        
    }
    
    private async Task<IActionResult> ExecuteProtectedAction(Guid userId, Guid organisationId, Func<Task<IActionResult>> action) 
    {
        var isUserAuthorised = _validationService.IsAuthorisedToManageComplianceScheme(userId, organisationId);

        if (!isUserAuthorised)
        {
            _logger.LogError($"User {userId} unauthorised to perform an action on behalf of organisation {organisationId}.");
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        return await action.Invoke();
    }
    
    private async Task<IActionResult> ExecuteProtectedComplianceSchemeAction(Guid userId, Guid organisationId, Func<Task<IActionResult>> action)
    {
        var isUserAuthorised = await _validationService.IsAuthorisedToViewComplianceSchemeMembers(userId, organisationId);

        if (!isUserAuthorised)
        {
            _logger.LogError($"User {userId} unauthorised to perform an action on behalf of compliance scheme organisation {organisationId}.");
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        return await action.Invoke();
    }
    
    [Route("member-removal-reasons")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceSchemeRemovalReasonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetComplianceSchemeReasonsForRemoval()
    {
        var result = await _complianceSchemeService.GetComplianceSchemeReasonsForRemovalAsync();
        _logger.LogInformation("Reasons for removal fetched successfully");
        return Ok(result);
    }
}