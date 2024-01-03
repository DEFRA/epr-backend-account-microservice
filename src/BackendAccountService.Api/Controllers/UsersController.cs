using System.Net;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Helpers;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService,
        IOptions<ApiConfig> baseApiConfigOptions, ILogger<UsersController> logger)
        : base(baseApiConfigOptions)
    {
        _userService = userService;
        _logger = logger;
    }

    [Route("user-organisations")]
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserOrganisation(Guid userId)
    {
        _logger.LogInformation("Fetching the list of organisations for the user id {userId}", userId);
        try
        {
            var result = await _userService.GetUserOrganisationAsync(userId);

            if (!result.IsSuccess)
            {
                _logger.LogInformation("Error getting user organisation details for the user id {userId}", userId);
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("User organisation details fetched successfully for the user id {userId}", userId);
            return new OkObjectResult(result.Value);
        }
        catch (Exception)
        {
            _logger.LogError("Exception fetching organisation details for the user id {userId}", userId);
            return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
        }
    }

    [HttpGet]
    [Route("user-account")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
}