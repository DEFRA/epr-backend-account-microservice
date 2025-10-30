using System.Net;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/accounts-management")]
public class AccountsManagementController : ApiControllerBase
{
    private readonly ILogger<AccountsManagementController> _logger;
    private readonly IAccountManagementService _accountManagementService;
    private readonly IValidationService _validateDataService;
    private readonly IUserService _userService;
    private readonly IEnrolmentsService _enrolmentsService;

    public AccountsManagementController(IAccountManagementService accountManagementService,
        ILogger<AccountsManagementController> logger,
        IOptions<ApiConfig> baseApiConfigOptions,
        IValidationService validateDataService,
        IUserService userService,
        IEnrolmentsService enrolmentsService)
        : base(baseApiConfigOptions)
    {
        _accountManagementService = accountManagementService;
        _logger = logger;
        _validateDataService = validateDataService;
        _userService = userService;
        _enrolmentsService = enrolmentsService;
    }

    [HttpPost]
    [Consumes("application/json")]
    [Route("invite-user")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InviteUser(AddInviteUserRequest request)
    {
        return await ExecuteProtectedAction(request.InvitingUser.UserId, request.InvitedUser.OrganisationId,
            request.InvitedUser.ServiceRoleId, async () =>
            {
                var canReInviteUser = false;
                
                var isUserPreviouslyInvited = await _validateDataService.IsUserInvitedAsync(request.InvitedUser.Email);
                
                if (isUserPreviouslyInvited)
                {
                    canReInviteUser = await CanReInviteUser(request);
                }

                if (!ModelState.IsValid)
                {
                    return ValidationProblem();
                }

                var inviteToken = isUserPreviouslyInvited && canReInviteUser
                    ? await _accountManagementService.ReInviteUserAsync(request.InvitedUser, request.InvitingUser)
                    : await _accountManagementService.CreateInviteeAccountAsync(request);

                if (string.IsNullOrWhiteSpace(inviteToken))
                {
                    _logger.LogError("Failed to add the invited user {InvitedUserUserId}", request.InvitedUser.UserId);
                    return Problem(statusCode: StatusCodes.Status500InternalServerError,
                        type: "Token not generated");
                }

                return new OkObjectResult(inviteToken);
            });
    }

    private async Task<bool> CanReInviteUser(AddInviteUserRequest request)
    {
        var isInvitedUserEnrolled = await _enrolmentsService.IsUserEnrolledAsync(request.InvitedUser.Email,
            request.InvitedUser.OrganisationId);

        if (!isInvitedUserEnrolled)
        {
            var invitedUserOrganisation = request.InvitedUser.OrganisationId;
            var invitingUserOrganisations =
                await _userService.GetUserOrganisationAsync(request.InvitingUser.UserId);

            if (invitingUserOrganisations.Value.User.Organisations.Select(x => x.Id)
                .Contains(invitedUserOrganisation))
            {
                return true;
            }

            ModelState.AddModelError(nameof(request.InvitedUser.Email),
                $"Invited user '{request.InvitedUser.Email}' doesn't belong to the same organisation.");
        }
        else
        {
            ModelState.AddModelError(nameof(request.InvitedUser.Email),
                $"Invited user '{request.InvitedUser.Email}' is enrolled already.");
        }

        return false;
    }

    [HttpPost]
    [Consumes("application/json")]
    [Route("enrol-invited-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EnrolInvitedUser(EnrolInvitedUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        var user = await _userService.GetUserByInviteAsync(request.Email, request.InviteToken);
        if (user == null)
        {
            _logger.LogError("Invite not found for user {UserId}", request.UserId);
            return Problem("Invite not found", statusCode: (int)HttpStatusCode.NoContent);
        }

        var success = await _accountManagementService.EnrolInvitedUserAsync(user, request);

        if (!success)
        {
            _logger.LogError("No pending enrolments to update for user {UserId}", request.UserId);
            return Problem("No pending enrolments to update", statusCode: (int)HttpStatusCode.NoContent);
        }

        return NoContent();
    }

    private async Task<IActionResult> ExecuteProtectedAction(Guid userId, Guid organisationId, int serviceRoleId, Func<Task<IActionResult>> action)
    {
        var isUserAuthorised = _validateDataService.IsAuthorisedToManageUsers(userId, organisationId, serviceRoleId);

        if (!isUserAuthorised)
        {
            _logger.LogError("User {UserId} unauthorised to perform an action on behalf of organisation {OrganisationId}.", userId, organisationId);
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        return await action.Invoke();
    }
}