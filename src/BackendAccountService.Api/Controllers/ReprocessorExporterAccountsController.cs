using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Core.Validation;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class ReprocessorExporterAccountsController(
    IAccountService accountService,
    IPersonService personsService,
    IPartnerService partnerService,
    IOrganisationService organisationService,
    IOptions<ApiConfig> baseApiConfigOptions)
    : ApiControllerBase(baseApiConfigOptions)
{
    [HttpPost]
    [Route("reprocessor-exporter-user-accounts")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAccount(
        ReprocessorExporterAccount account,
        [BindRequired, FromQuery] string serviceKey,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId)
    {
        // we can null-forgive UserId, as a null UserId is caught at the model binding stage before this method is called
        var existingPerson = await personsService.GetPersonResponseByUserId(account.User.UserId!.Value);
        if (existingPerson != null)
        {
            ModelState.AddModelError(nameof(account.User.UserId),
                $"User '{account.User.UserId}' already exists");

            return ValidationProblem(
                statusCode: StatusCodes.Status409Conflict,
                detail: "User already exists",
                type: "create-reprocessor-exporter-account/user-exists");
        }

        await accountService.AddReprocessorExporterAccountAsync(account, serviceKey, userId);

        return Ok();
    }

    /// <remarks>
    /// We can't check if the person is authorised to manage users (ExecuteProtectedAction),
    /// as the person created through the re-ex CreateAccount handler above
    /// (called at the end of the add re-ex user journey) doesn't belong to an organisation.
    ///
    /// For non company house organisations, there's no reliable way to check if the organisation already exists.
    /// As reporting is a legal requirement for organisations, it's better to allow duplicates
    /// than to block the user from creating an organisation.
    /// </remarks>
    [HttpPost]
    [Route("reprocessor-exporter-accounts")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReExAddOrganisationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddOrganisation(
        ReprocessorExporterAddOrganisation organisation,
        [BindRequired, FromQuery] string serviceKey,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid auditUserId)
    {
        var partnerRoles = await partnerService.GetPartnerRoles();
        var existingPerson = await personsService.GetPersonByUserId(organisation.User.UserId);

        var validationProblem = await ValidateUserIdsMatch(organisation, auditUserId)
            .ThenValidateAsync(() => ValidateCompaniesHouseNumber(organisation))
            .ThenValidate(() => ValidatePersonExists(organisation, existingPerson))
            .ThenValidate(() => ValidateInvitedApprovedUsersAreDistinct(organisation))
            .ThenValidate(() => ValidatePartnerRoles(organisation, partnerRoles));

        if (validationProblem != null)
        {
            return validationProblem;
        }

        return Ok(await accountService.AddReprocessorExporterOrganisationAsync(
            organisation,
            existingPerson!,
            partnerRoles,
            serviceKey,
            auditUserId));
    }

    private ActionResult? ValidateUserIdsMatch(ReprocessorExporterAddOrganisation organisation, Guid auditUserId)
    {
        if (auditUserId == organisation.User.UserId)
        {
            return null;
        }

        ModelState.AddModelError(nameof(organisation.User.UserId),
            $"UserId '{organisation.User.UserId}' doesn't match audit UserId '{auditUserId}'");

        return ValidationProblem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: "UserId doesn't match audit UserId",
            type: "add-organisation/user-id-mismatch");
    }

    private async Task<ActionResult?> ValidateCompaniesHouseNumber(ReprocessorExporterAddOrganisation organisation)
    {
        if (string.IsNullOrEmpty(organisation.Organisation.CompaniesHouseNumber))
        {
            return null;
        }

        var organisationList = await organisationService.GetOrganisationsByCompaniesHouseNumberAsync(organisation.Organisation.CompaniesHouseNumber);
        if (organisationList.Count == 0)
        {
            return null;
        }

        ModelState.AddModelError(nameof(organisation.Organisation.CompaniesHouseNumber),
            $"Organisation with the same Companies House number '{organisation.Organisation.CompaniesHouseNumber}' already exists");

        return ValidationProblem(
            statusCode: StatusCodes.Status409Conflict,
            detail: "Organisation already exists",
            type: "add-organisation/organisation-exists");
    }

    private ActionResult? ValidatePersonExists(ReprocessorExporterAddOrganisation organisation, Person? existingPerson)
    {
        if (existingPerson != null)
        {
            return null;
        }

        ModelState.AddModelError(nameof(organisation.User.UserId),
            $"Person with UserId '{organisation.User.UserId}' does not exist");

        return ValidationProblem(
            statusCode: StatusCodes.Status409Conflict,
            detail: "Person does not exist",
            type: "add-organisation/person-does-not-exist");
    }

    private ActionResult? ValidatePartnerRoles(
        ReprocessorExporterAddOrganisation organisation,
        IImmutableDictionary<string, PartnerRole> partnerRoles)
    {
        foreach (var partnerRoleName in organisation.Partners.Select(p => p.PartnerRole))
        {
            if (!partnerRoles.ContainsKey(partnerRoleName))
            {
                ModelState.AddModelError(nameof(PartnerModel.PartnerRole),
                    $"Partner role '{partnerRoleName}' doesn't exist");
            }
        }

        if (ModelState.ErrorCount == 0)
        {
            return null;
        }

        return ValidationProblem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: "Partner role(s) does not exist",
            type: "add-organisation/invalid-partner-role");
    }

    private ActionResult? ValidateInvitedApprovedUsersAreDistinct(ReprocessorExporterAddOrganisation organisation)
    {
        if (organisation.InvitedApprovedUsers.Count ==
            organisation.InvitedApprovedUsers.DistinctBy(u => u.Person.ContactEmail).Count())
        {
            return null;
        }

        ModelState.AddModelError(nameof(organisation.InvitedApprovedUsers), "Can't invite user multiple times");

        return ValidationProblem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: "Can't invite user multiple times",
            type: "add-organisation/duplicate-invited-users");
    }
}
