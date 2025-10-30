using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Helpers;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/organisations")]
public class OrganisationsController : ApiControllerBase
{
    private readonly IOrganisationService _organisationService;
    private readonly ILogger<OrganisationsController> _logger;
    private readonly IValidationService _validateDataService;
    private readonly IUserService _userService;
    private readonly IRegulatorService _regulatorService;

    private User _user;
    private OrganisationResponseModel _organisation;

    public OrganisationsController(IOrganisationService organisationService, IOptions<ApiConfig> baseApiConfigOptions,
        ILogger<OrganisationsController> logger,
        IValidationService validateDataService,
        IUserService userService,
        IRegulatorService regulatorService)
        : base(baseApiConfigOptions)
    {
        _organisationService = organisationService;
        _logger = logger;
        _validateDataService = validateDataService;
        _userService = userService;
        _regulatorService = regulatorService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrganisationResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationsByCompaniesHouseNumber(string companiesHouseNumber)
    {
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync(companiesHouseNumber);
        if (organisationList.Count > 0)
        {
            return Ok(organisationList);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Route("organisation-by-companies-house-number")]
    [ProducesResponseType(typeof(OrganisationResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCompaniesHouseNumber(string companiesHouseNumber)
    {
        var organisation = await _organisationService.GetByCompaniesHouseNumberAsync(companiesHouseNumber);

        if (organisation != null)
        {
            return Ok(organisation);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Consumes("application/json")]
    [Route("users")]
    [ProducesResponseType(typeof(IQueryable<OrganisationUsersResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Users(Guid userId, Guid organisationId, int serviceRoleId)
    {
        return await ExecuteProtectedAction(userId, organisationId, 3, async () =>
        {
            var result = await _organisationService.GetUserListForOrganisation(userId, organisationId, serviceRoleId);

            return new OkObjectResult(result);
        });
    }

    [HttpGet]
    [Consumes("application/json")]
    [Route("all-users")]
    [ProducesResponseType(typeof(IQueryable<OrganisationUsersResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AllUsers(Guid userId, Guid organisationId, int serviceRoleId)
    {
        var result = await _organisationService.GetUserListForOrganisation(userId, organisationId, serviceRoleId);
        return new OkObjectResult(result);
    }

	[HttpGet]
	[Consumes("application/json")]
	[Route("team-members")]
	[ProducesResponseType(typeof(IQueryable<TeamMembersResponseModel>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> TeamMembers(Guid userId, Guid organisationId, int serviceRoleId)
	{
		var result = await _organisationService.GetTeamMemberListForOrganisation(userId, organisationId, serviceRoleId);
		return new OkObjectResult(result);
	}

	[HttpGet]
    [Route("organisation-by-externalId")]
    [ProducesResponseType(typeof(OrganisationDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationByExternalIdAsync(Guid externalId)
    {
        var organisation = await _organisationService.GetOrganisationByExternalId(externalId);

        if (organisation != null)
        {
            return Ok(organisation);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Route("organisation-by-invite-token")]
    [ProducesResponseType(typeof(ApprovedPersonOrganisationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationByExternalIdAsync([Required] string token)
    {
        var organisation = await _organisationService.GetOrganisationNameByInviteTokenAsync(token);

        if (organisation != null)
        {
            return Ok(organisation);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpPost]
    [Route("create-and-add-subsidiary")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAndAddSubsidiary(
   [BindRequired, FromBody] LinkOrganisationRequestModel request)
    {
        return await ExecuteSubsidiaryAction(
            request.UserId,
            request.ParentOrganisationId,
            async () =>
            {
                var organisationRelationshipModel = new OrganisationRelationshipModel
                {
                    FirstOrganisationId = _organisation.Id,
                    OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
                    OrganisationRegistrationTypeId = null,
                    CreatedOn = DateTime.UtcNow,
                    RelationFromDate = DateTime.UtcNow,
                    LastUpdatedById = _user.Id,
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedByOrganisationId = _organisation.Id
                };

                var result = await _organisationService.AddOrganisationAndOrganisationRelationshipsAsync(request.Subsidiary,
                                        organisationRelationshipModel, request.UserId);

                return Ok(result.ReferenceNumber);
            });
    }

    [HttpPost]
    [Route("add-subsidiary")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddSubsidiary(SubsidiaryAddRequestModel request)
    {
        return await ExecuteSubsidiaryAction(request.UserId, request.ParentOrganisationId, async () =>
        {
            var organisationRelationshipModel = new OrganisationRelationshipModel
            {
                FirstOrganisationId = _organisation.Id,
                OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
                OrganisationRegistrationTypeId = null,
                CreatedOn = DateTime.UtcNow,
                RelationFromDate = DateTime.UtcNow,
                LastUpdatedById = _user.Id,
                LastUpdatedOn = DateTime.UtcNow,
                LastUpdatedByOrganisationId = _organisation.Id
            };

            var childOrganisation = await _organisationService.GetOrganisationResponseByExternalId(request.ChildOrganisationId);

            if (childOrganisation is null)
            {
                return ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "child organisation does not exist",
                type: "child organisation/invalid-childOrganisationId");
            }

            organisationRelationshipModel.SecondOrganisationId = childOrganisation.Id;

            var result = await _organisationService.AddOrganisationRelationshipsAsync(organisationRelationshipModel, request.ParentOrganisationId, request.UserId);

            return Ok(result != null ? childOrganisation!.ReferenceNumber : null);
        });
    }

    [HttpPost]
    [Route("terminate-subsidiary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TerminateSubsidiary(SubsidiaryTerminateRequestModel request)
    {
        return await ExecuteSubsidiaryAction(request.UserId, request.ParentOrganisationId, async () =>
        {
            var childOrganisation = await _organisationService.GetOrganisationResponseByExternalId(request.ChildOrganisationId);
            if (childOrganisation is null)
            {
                return ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "child organisation does not exist",
                type: "child organisation/invalid-childOrganisationId");
            }

            var terminateRequest = new TerminateSubsidiaryModel
            {
                ParentExternalId = request.ParentOrganisationId,
                ParentOrganisationId = _organisation.Id,
                ChildOrganisationId = childOrganisation.Id,
                UserExternalId = request.UserId,
                UserId = _user.Id
            };

            var result = await _organisationService.TerminateOrganisationRelationshipsAsync(terminateRequest);
            if (!result.IsSuccess)
            {
                _logger.LogInformation("Error removing the selected subsidiary {ChildOrganisationId} from Organisation {ParentOrganisationId}", request.ChildOrganisationId, request.ParentOrganisationId);
                return result.BuildErrorResponse();
            }

            _logger.LogInformation("Removed the selected subsidiary {ChildOrganisationId} from Organisation {ParentOrganisationId}", request.ChildOrganisationId, request.ParentOrganisationId);

            return Ok();
        });
    }

    [HttpGet]
    [Route("{organisationId:guid}/organisationRelationships")]
    [ProducesResponseType(typeof(OrganisationRelationshipResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationRelationshipsByOrganisationIdAsync(Guid organisationId)
    {
        var organisationRelationships = await _organisationService.GetOrganisationRelationshipsByOrganisationId(organisationId);

        if (organisationRelationships != null)
        {
            return Ok(organisationRelationships);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet]
    [Route("organisationRelationships")]
    [ProducesResponseType(typeof(PagedOrganisationRelationshipsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationRelationshipsAsync([Required] int? page, [Required] int? showPerPage, string search = null)
    {
        var result = await _organisationService.GetPagedOrganisationRelationships(page.Value, showPerPage.Value, search);

        return Ok(result);
    }

    [HttpGet]
    [Route("organisationRelationshipsWithoutPaging")]
    [ProducesResponseType(typeof(List<RelationshipResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationRelationshipsWithoutPagingAsync()
    {
        var result = await _organisationService.GetUnpagedOrganisationRelationships();

        return Ok(result);
    }

    [HttpGet]
    [Route("{organisationId:guid}/export-subsidiaries")]
    [ProducesResponseType(typeof(List<ExportOrganisationSubsidiariesResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportDirectProducerSubsidiaries(Guid organisationId)
    {
        var organisationRelationships = await _organisationService.ExportOrganisationSubsidiaries(organisationId);
        if (organisationRelationships != null)
        {
            return Ok(organisationRelationships);
        }
        else
        {
            return NoContent();
        }
    }
    private async Task<IActionResult> ExecuteSubsidiaryAction(Guid userId, Guid organisationId, Func<Task<IActionResult>> action)
    {
        _organisation = await _organisationService.GetOrganisationResponseByExternalId(organisationId);

        if (_organisation is null)
        {
            ModelState.AddModelError(nameof(organisationId),
                $"organisation '{organisationId}' does not exist");
            return ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "organisation does not exist",
                type: "organisation/invalid-externalId");
        }

        _user = await _userService.GetUserByUserId(userId);

        if (_user is null)
        {
            ModelState.AddModelError(nameof(userId),
               $"user '{userId}' does not exist");
            return ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "user does not exist",
                type: "user/invalid-userId");
        }
        return await action.Invoke();
    }

    [HttpGet]
    [Route("organisation-by-reference-number")]
    [ProducesResponseType(typeof(OrganisationResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationResponseByReferenceNumberAsync([Required] string referenceNumber)
    {
        var organisation = await _organisationService.GetOrganisationResponseByReferenceNumber(referenceNumber);

        if (organisation != null)
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


    /// <summary>
    /// Updates an organisation.
    /// </summary>
    /// <param name="organisationId">The id of the organisation to update</param>
    /// <param name="organisation">The new organisation details to update for the organisation</param>
    /// <param name="userId">ID of the user performing the operation</param>
    /// <returns>Async IAction result indicating success or failure</returns>
    [HttpPut]
    [Route("organisation/{organisationId}")]
    [ProducesResponseType(typeof(OrganisationResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateNationIdByOrganisationId(
        Guid organisationId,
        [FromBody] OrganisationUpdateModel? organisation,
        [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId)
    {
        if (organisation == null)
        {
            return BadRequest();
        }

        var result = await _organisationService.UpdateOrganisationDetails(
            userId,
            organisationId,
            organisation);

        if (!result.IsSuccess)
        {
            return result.BuildErrorResponse();
        }
        return Ok();
    }


    [HttpGet]
    [Route("organisation")]
    [ProducesResponseType(typeof(List<UpdatedProducersResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUpdatedProducers([FromQuery] UpdatedProducersRequest request)
    {
        var organisation = await _organisationService.GetUpdatedProducers(request);
        return organisation.Count > 0 ? Ok(organisation) : NoContent();
    }

    [HttpGet]
    [Route("nation-code")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganisationNationByExternalId([FromQuery] Guid organisationId)
    {
        var isValidCSOrganisation = false;
        var isValidOrganisation = await _organisationService.IsOrganisationValidAsync(organisationId);

        if (!isValidOrganisation)
        {
            isValidCSOrganisation = await _organisationService.IsCSOrganisationValidAsync(organisationId);
        }

        if (isValidOrganisation)
        {
            _logger.LogInformation("Retrieving the nation list for an organisation: {OrganisationId}", organisationId);

            var response = await _regulatorService.GetOrganisationNationsAsync(organisationId);
            if (response is null) return NotFound("Organisation not found");

            var nationCode = response.Select(x => x.NationCode).FirstOrDefault();

            return Ok(nationCode);
        }

        if (isValidCSOrganisation)
        {
            _logger.LogInformation("Retrieving the nation list for a Compliance Scheme organisation: {OrganisationId}", organisationId);

            var response = await _regulatorService.GetCSOrganisationNationsAsync(organisationId);
            if (response is null) return NotFound("Organisation not found");

            var nationCode = response.Select(x => x.NationCode).FirstOrDefault();

            return Ok(nationCode);
        }

        return NoContent();
    }

    [HttpGet]
    [Route("person-emails")]
    [ProducesResponseType(typeof(List<PersonEmailResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPersonEmailsAsync(Guid organisationId, string entityTypeCode)
    {
        if (organisationId == Guid.Empty)
        {
            return BadRequest();
        }

        var personEmails = await _organisationService.GetPersonEmails(organisationId, entityTypeCode);
        return personEmails.Count > 0 ? Ok(personEmails) : NoContent();
    }

    [HttpGet]
    [Route("validate-issued-epr-id")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateProducerEprId(Guid externalId, string entityTypeCode)
    {
        if (externalId == Guid.Empty)
            return BadRequest();

        return _validateDataService.IsExternalIdExists(externalId, entityTypeCode)
            ? Ok(externalId)
            : NoContent();
	}
}