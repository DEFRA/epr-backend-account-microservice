using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers
{
    [ApiController]
    [Consumes("application/json")]
    [Route("api/regulator-accounts")]
    public class RegulatorAccountsController : ApiControllerBase
    {
        private readonly ILogger<RegulatorAccountsController> _logger;
        private readonly IAccountManagementService _accountManagementService;
        private readonly IValidationService _validateDataService;

        public RegulatorAccountsController(IOptions<ApiConfig> baseApiConfigOptions,
            IAccountManagementService accountManagementService,
            IValidationService validateDataService,
            ILogger<RegulatorAccountsController> logger) : base(baseApiConfigOptions)
        {
            _logger = logger;
            _validateDataService = validateDataService;
            _accountManagementService = accountManagementService;
        }

        [HttpPost]
        [Route("invite-user")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InviteUser(AddInviteUserRequest request)
        {
            var isUserInvited = await _validateDataService.IsUserInvitedAsync(request.InvitedUser.Email);

            if (isUserInvited)
            {
                ModelState.AddModelError(nameof(request.InvitedUser.Email),
                    $"User '{request.InvitedUser.Email}' is already invited");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            var result = await _accountManagementService.CreateInviteeAccountAsync(request);

            if (string.IsNullOrWhiteSpace(result))
            {
                _logger.LogError("Failed to add the invited user {UserId}", request.InvitedUser.UserId);
                return Problem(statusCode: StatusCodes.Status500InternalServerError, type: "Token not generated");
            }

            return new OkObjectResult(result);
        }

        [HttpGet]
        [Route("invited-user")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InvitedUser(Guid userId, string email)
        {
            var invitedUserToken = await _validateDataService.UserInvitedTokenAsync(userId);
            _logger.LogInformation("Returning invite token {InvitedUserToken}", invitedUserToken);
            return new OkObjectResult(invitedUserToken);
        }
    }
}
