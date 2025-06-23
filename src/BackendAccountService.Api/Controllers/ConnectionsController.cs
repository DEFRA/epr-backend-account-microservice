using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Exceptions;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/connections")]
public class ConnectionsController : ApiControllerBase
{
    private readonly IValidationService _validateDataService;
    private readonly IRoleManagementService _roleManagementService;
    private readonly ILogger<ConnectionsController> _logger;

    public ConnectionsController(
        IValidationService validateDataService,
        IRoleManagementService roleManagementService,
        IOptions<ApiConfig> baseApiConfigOptions,
        ILogger<ConnectionsController> logger)
        : base(baseApiConfigOptions)
    {
        _validateDataService = validateDataService;
        _roleManagementService = roleManagementService;
        _logger = logger;
    }

    [HttpGet]
    [Route("{connectionId:guid}/roles")]
    [ProducesResponseType(typeof(ConnectionWithEnrolmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConnectionAndEnrolments(
        Guid connectionId,
        [BindRequired, FromQuery] string serviceKey,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        var isAuthorised = await _validateDataService.IsAuthorisedToManageUsersFromOrganisationForService(userId, organisationId, serviceKey);
        if (!isAuthorised)
        {
            _logger.LogError(
                "User {UserId} from organisation {OrganisationId} is not authorised to access enrolments for connection {ConnectionId} to service {ServiceKey}",
                userId, organisationId, connectionId, serviceKey);
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        var response = await _roleManagementService.GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(connectionId, organisationId, serviceKey);
        if (response == null)
        {
            _logger.LogError(
                "Connection {ConnectionId} from organisation {OrganisationId} could not be found",
                connectionId, organisationId);
            return Problem(statusCode: StatusCodes.Status404NotFound, type: "connection-not-found");
        }

        return new OkObjectResult(response);
    }

    [HttpGet]
    [Route("{connectionId:guid}/person")]
    [ProducesResponseType(typeof(ConnectionWithPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConnectionWithPersonResponse>> GetConnectionAndPerson(
       Guid connectionId,
       [BindRequired, FromQuery] string serviceKey,
       [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
       [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        var isAuthorised = await _validateDataService.IsAuthorisedToManageUsersFromOrganisationForService(userId, organisationId, serviceKey);
        if (!isAuthorised)
        {
            _logger.LogError("User {UserId} from organisation {OrganisationId} is not authorised to access enrolments for connection {ConnectionId} to service {ServiceKey}", userId, organisationId, connectionId, serviceKey);
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        var response = await _roleManagementService.GetConnectionWithPersonForServiceAsync(connectionId, organisationId, serviceKey);

        if (response == null)
        {
            _logger.LogError("Connection {ConnectionId} from organisation {OrganisationId} could not be found", connectionId, organisationId);
            return Problem(statusCode: StatusCodes.Status404NotFound, type: "connection-not-found");
        }

        return new OkObjectResult(response);
    }

    [HttpPut]
    [Consumes("application/json")]
    [Route("{connectionId:guid}/roles")]
    [ProducesResponseType(typeof(UpdatePersonRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePersonRole(
        Guid connectionId,
        UpdatePersonRoleRequest updateRequest,
        [BindRequired, FromQuery] string serviceKey,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        if (serviceKey != "Packaging")
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, type: "service not supported");
        }

        var isAuthorised = await _validateDataService.IsAuthorisedToManageUsersFromOrganisationForService(userId, organisationId, serviceKey);
        if (!isAuthorised)
        {
            _logger.LogError(
                "User {UserId} from organisation {OrganisationId} is not authorised to access enrolments for connection {ConnectionId} to service {ServiceKey}",
                userId, organisationId, connectionId, serviceKey);
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        try
        {
            var result = await _roleManagementService.UpdatePersonRoleAsync(connectionId, userId, organisationId, serviceKey, updateRequest.PersonRole);

            return new OkObjectResult(result);
        }
        catch (RoleManagementException ex)
        {
            _logger.LogError( 
                ex,
                "User {UserId} from organisation {OrganisationId} failed to update person role '{PersonRole}' for connection {ConnectionId} of service '{ServiceKey}': {Message}",
                userId, 
                organisationId, 
                updateRequest.PersonRole, 
                connectionId, 
                serviceKey,
                ex.Message);

            return Problem(
                title: "Failed to update Person Role",
                statusCode: StatusCodes.Status400BadRequest,
                type: "person-role",
                detail: ex.Message);
        }
    }

    [HttpPut]
    [Consumes("application/json")]
    [Route("{connectionId:guid}/delegated-person-nomination")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NominateToDelegatedPerson(
        Guid connectionId,
        [BindRequired, FromQuery] string serviceKey,
        [BindRequired, FromBody] DelegatedPersonNominationRequest nominationRequest,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        if (serviceKey != "Packaging")
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, type: "service-not-supported");
        }

        var isAuthorised = await _validateDataService.IsAuthorisedToManageDelegatedUsersFromOrganisationForService(userId, organisationId, serviceKey);
        if (!isAuthorised)
        {
            _logger.LogError(
                "User {UserId} from organisation {OrganisationId} is not authorised to access enrolments for connection {ConnectionId} to service {ServiceKey}",
                userId, organisationId, connectionId, serviceKey);
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        var result = await _roleManagementService.NominateToDelegatedPerson(connectionId, userId, organisationId, serviceKey, nominationRequest);

        if (result.Succeeded)
        {
            return Ok();
        }
        
        _logger.LogError(
            "User {UserId} from organisation {OrganisationId}, service '{ServiceKey}' failed to nominate person of connection {ConnectionId} to Delegated Person. Error: {ErrorMessage}", 
            userId, organisationId, serviceKey, connectionId,  result.ErrorMessage);
            
        return Problem(
            title: "Failed to nominate user to Delegated Person",
            statusCode: StatusCodes.Status400BadRequest, 
            type: "delegated-person-invitation",
            detail: result.ErrorMessage);
    }
}
