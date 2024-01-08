using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/producer-accounts")]
public class AccountsController : ApiControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IPersonService _personsService;
    private readonly IOrganisationService _organisationService;
    private readonly IUserService _userService;

    public AccountsController(
        IAccountService accountService, 
        IPersonService personsService,
        IOrganisationService organisationService,
        IUserService userService,
        IOptions<ApiConfig> baseApiConfigOptions)
        : base(baseApiConfigOptions)
    {
        _accountService = accountService;
        _personsService = personsService;
        _organisationService = organisationService;
        _userService = userService;
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount(AccountModel account)
    {
        var serviceRole = await _accountService.GetServiceRoleAsync(account.Connection.ServiceRole);
        if (serviceRole == null)
        {
            ModelState.AddModelError(nameof(account.Connection.ServiceRole),
                $"Service role '{account.Connection.ServiceRole}' does not exist");
            return ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Service role does not exist",
                type: "create-account/invalid-service-role");
        }

        if (!string.IsNullOrEmpty(account.Organisation.CompaniesHouseNumber))
        {
            var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync(account.Organisation.CompaniesHouseNumber);
            if (organisationList.Any())
            {
                ModelState.AddModelError(nameof(account.Organisation.CompaniesHouseNumber),
                    $"Organisation with the same Companies House number '{account.Organisation.CompaniesHouseNumber}' already exists");

                return ValidationProblem(
                    statusCode: StatusCodes.Status409Conflict,
                    detail: "Organisation already exists",
                    type: "create-account/organisation-exists");
            }
        }
        
        var existingPerson = await _personsService.GetPersonByUserIdAsync(account.User.UserId.Value);
        if (existingPerson != null)
        {
            ModelState.AddModelError(nameof(account.User.UserId),
                $"User '{account.User.UserId}' already exists");

            return ValidationProblem(
                statusCode: StatusCodes.Status409Conflict, 
                detail: "User already exists", 
                type: "create-account/user-exists");
        }

        var enrolment = await _accountService.AddAccountAsync(account, serviceRole);

        return Ok(new CreateAccountResponse
        {
            ReferenceNumber = enrolment.Connection.Organisation.ReferenceNumber!,
            OrganisationId = enrolment.Connection.Organisation.ExternalId
        });
    }
    
    [HttpPost]
    [Route("ApprovedUser")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateApprovedUserAccount(ApprovedUserAccountModel account)
    {
        var serviceRole = await _accountService.GetServiceRoleAsync(account.Connection.ServiceRole);
        if (serviceRole == null)
        {
            ModelState.AddModelError(nameof(account.Connection.ServiceRole),
                $"Service role '{account.Connection.ServiceRole}' does not exist");
            return ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Service role does not exist",
                type: "create-account/invalid-service-role");
        }
        
        var user = await _userService.GetApprovedUserUserByEmailAsync(account.Person.ContactEmail);
        if (user == null)
        {
            ModelState.AddModelError(nameof(account.Person.ContactEmail),
                $"email '{account.Person.ContactEmail}' does not exist");
            return ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "email does not exist",
                type: "create-account/invalid-service-role");
        }
        
        var enrolment = await _accountService.AddApprovedUserAccountAsync(account, serviceRole, user);

        return Ok(new CreateAccountResponse
        {
            ReferenceNumber = enrolment.Connection.Organisation.ReferenceNumber!,
            OrganisationId = enrolment.Connection.Organisation.ExternalId
        });
    }
}