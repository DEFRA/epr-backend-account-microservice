using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Helpers;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService,
        IOptions<ApiConfig> baseApiConfigOptions,
        ILogger<UsersController> logger)
        : base(baseApiConfigOptions)
    {

        _userService = userService;
        _logger = logger;
    }

    [Route("user-organisations")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType<UserOrganisationsListModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserOrganisation(Guid userId)
    {
        return await GetUserOrganisationInfo(userId);
    }

    [Route("user-organisations-include-deleted")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType<UserOrganisationsListModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserOrganisationIncludeDeleted(Guid userId)
    {
        return await GetUserOrganisationInfo(userId, true);
    }

    [HttpGet("v1/user-organisations")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UserOrganisationsListModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserOrganisationsWithEnrolments([FromQuery] Guid userId, [FromQuery] string serviceKey = null)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest("The userId parameter is required.");
        }

        _logger.LogInformation("Fetching the list of organisations for the user id {UserId}", userId);

        try
        {
            var result = await _userService.GetUserOrganisationsWithEnrolmentsAsync(userId, serviceKey);

            if (!result.IsSuccess)
            {
                _logger.LogInformation("Error getting user organisation details for the user id {UserId}", userId);
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("User organisation details fetched successfully for the user id {UserId}", userId);
            return new OkObjectResult(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching organisation details for the user id {UserId}", userId);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [Route("user-organisation-ids")]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UserOrganisationIdentifiersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserOrganisationIds(UserOrganisationIdentifiersRequest request)
    {
        _logger.LogInformation("GetUserOrganisationIds:Called with objectId {ObjectId}", request.ObjectId);

        var userId = request.ObjectId;
        var organisationIds = string.Empty;

        try
        {
            var result = await _userService.GetUserOrganisationIdListAsync(userId);

            if (!result.IsSuccess)
            {
                _logger.LogInformation("GetUserOrganisationIds:Failed to get user organisation details for user {UserId}, status code {StatusCode}, error {ErrorMessage}", userId, result.StatusCode, result.ErrorMessage);
            }
            else
            {
                organisationIds = string.Join(",", result.Value);
            }

            _logger.LogInformation("GetUserOrganisationIds:Returning organisations details {OrganisationIds} for user {UserId}", organisationIds, userId);
            return new OkObjectResult(new UserOrganisationIdentifiersResponse
            {
                OrganisationIds = organisationIds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching organisation details for user id {UserId}", userId);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpGet]
    [Route("user-account")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PersonWithOrganisationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPersonOrganisations(
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
        [BindRequired, FromQuery] string serviceKey)
    {
        var result = await _userService.GetPersonOrganisationsWithEnrolmentsForServiceAsync(userId, serviceKey);

        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            return result.BuildErrorResponse();
        }

        return Problem(
            type: "internalservererror",
            title: "Unhandled exception",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet]
    [Route("user-by-person-id")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserIdByPersonId(Guid personId)
    {
        try
        {
            var userId = await _userService.GetUserIdByPersonId(personId);
            if (userId != null)
            {
                return Ok(userId);
            }
            else
            {
                return NoContent();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching user id for person id {PersonId}", personId);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPut]
    [Consumes("application/json")]
    [Route("personal-details")]
    [ProducesResponseType(typeof(UpdateUserDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdatePersonalDetails(
    [BindRequired, FromQuery] string serviceKey,
    [BindRequired, FromBody] UpdateUserDetailsRequest updateUserDetailsRequest,
    [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
    [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        if (serviceKey != "Packaging")
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: $"Unsupported service '{serviceKey}'", type: "service-not-supported");
        }

        if (updateUserDetailsRequest == null)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: $"user change details cannot be null", type: "user-change-details-empty");
        }

        var result = await _userService.UpdateUserDetailsRequest(userId, organisationId, serviceKey, updateUserDetailsRequest);

        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }
        return result.BuildErrorResponse();
    }

    [Route("system-user-and-organisation")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UserOrganisation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSystemUserAndOrganisation()
    {
        _logger.LogInformation("Fetching system UserId and OrganisationId");

        try
        {
            var result = await _userService.GetSystemUserAndOrganisationAsync(ApplicationUser.System);

            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogInformation("Error getting system user and Organisation");
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("System User and organisation fetched successfully");
            return new OkObjectResult(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching system user and organisation");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task<IActionResult> GetUserOrganisationInfo(Guid userId, bool includeDeleted = false)
    {
        _logger.LogInformation("Fetching the list of organisations for the user id {UserId}", userId);
        try
        {
            var result = await _userService.GetUserOrganisationAsync(userId, includeDeleted);

            if (!result.IsSuccess)
            {
                _logger.LogInformation("Error getting user organisation details for the user id {UserId}", userId);
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("User organisation details fetched successfully for the user id {UserId}", userId);
            return new OkObjectResult(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching organisation details for the user id {UserId}", userId);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}