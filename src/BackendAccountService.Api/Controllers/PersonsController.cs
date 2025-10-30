using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/persons")]
public class PersonsController : ApiControllerBase
{
    private readonly IPersonService _personService;
    private readonly IUserService _userService;

    public PersonsController(IPersonService personService, IOptions<ApiConfig> baseApiConfigOptions, IUserService userService)
        : base(baseApiConfigOptions)
    {
        _personService = personService;
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PersonResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPersonByUserId(Guid userId)
    {
        var person = await _personService.GetPersonResponseByUserId(userId);
        if (person != null)
        {
            return Ok(person);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Route("allpersons")]
    [ProducesResponseType(typeof(PersonResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllPersonByUserId(Guid userId)
    {
        var person = await _personService.GetAllPersonByUserIdAsync(userId);
        if (person != null)
        {
            return Ok(person);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Route("person-by-externalId")]
    [ProducesResponseType(typeof(PersonResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPersonByExternalIdAsync([Required] Guid externalId)
    {
        var person = await _personService.GetPersonByExternalIdAsync(externalId);
        if (person != null)
        {
            return Ok(person);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Route("person-by-invite-token")]
    [ProducesResponseType(typeof(InviteApprovedUserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPersonByInviteTokenAsync([Required] string token)
    {
        var person = await _personService.GetPersonServiceRoleByInviteTokenAsync(token);

        if (person != null)
        {
            return Ok(person);
        }

        var invitationTokenExists = await _userService.InvitationTokenExists(token);
        if (invitationTokenExists)
        {
            var inviteApprovedUserModel = new InviteApprovedUserModel
            {
                IsInvitationTokenInvalid = true
            };
            return Ok(inviteApprovedUserModel);
        }

        return NoContent();
    }
}
