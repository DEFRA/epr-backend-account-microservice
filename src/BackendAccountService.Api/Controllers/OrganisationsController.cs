using System.ComponentModel.DataAnnotations;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/organisations")]
public class OrganisationsController : ApiControllerBase
{
    private readonly IOrganisationService _organisationService;
    private readonly ILogger<OrganisationsController> _logger;
    private readonly IValidationService _validateDataService;

    public OrganisationsController(IOrganisationService organisationService, IOptions<ApiConfig> baseApiConfigOptions,
        ILogger<OrganisationsController> logger,
        IValidationService validateDataService)
        : base(baseApiConfigOptions)
    {
        _organisationService = organisationService;
        _logger = logger;
        _validateDataService = validateDataService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationsByCompaniesHouseNumber(string companiesHouseNumber)
    {
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync(companiesHouseNumber);
        if (organisationList.Any())
        {
            return Ok(organisationList);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Consumes("application/json")]
    [Route("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Users(Guid userId, Guid organisationId, int serviceRoleId)
    {
        return await ExecuteProtectedAction(userId, organisationId, 3, async () =>
        {

            var result = await _organisationService.GetUserListForOrganisation(userId, organisationId, serviceRoleId);

            return new OkObjectResult(result);
        });
    }

    [HttpGet]
    [Route("organisation-by-externalId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationByExternalIdAsync(Guid externalId)
    {
        var organisation = await _organisationService.GetOrganisationByExternalId(externalId);
        
        if(organisation != null)
        {
            return Ok(organisation);
        }
        else
        {
            return NoContent();
        }
    }
    
    private async Task<IActionResult> ExecuteProtectedAction(Guid userId, Guid organisationId, int serviceRoleId, Func<Task<IActionResult>> action)
    {
        var isUserAuthorised = _validateDataService.IsAuthorisedToManageUsers(userId, organisationId, serviceRoleId);

        if (!isUserAuthorised)
        {
            _logger.LogError("User {UserId} unauthorised to perform an action on behalf of organisation {OrganisationId}", userId, organisationId);
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        return await action.Invoke();
    }
    
    [HttpGet]
    [Route("organisation-by-invite-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationByExternalIdAsync([Required]string token)
    {
        var organisation = await _organisationService.GetOrganisationNameByInviteTokenAsync(token);
        
        if(organisation != null)
        {
            return Ok(organisation);
        }
        else
        {
            return NoContent();
        }
    }
}
