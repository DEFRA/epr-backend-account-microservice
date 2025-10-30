using System;
using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Helpers;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/bulkuploadorganisations")]
public partial class BulkUploadController : ApiControllerBase
{
    private readonly IOrganisationService _organisationService;
    private readonly IUserService _userService;
    private User _user;
    private OrganisationResponseModel _organisation;
    private readonly ILogger<BulkUploadController> _logger;

    public BulkUploadController(IOrganisationService organisationService,
        IOptions<ApiConfig> baseApiConfigOptions,
        ILogger<BulkUploadController> logger,
        IValidationService validateDataService,
        IUserService userService)
        : base(baseApiConfigOptions)
    {
        _organisationService = organisationService;
        _userService = userService;
        _logger = logger;
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
    [Route("organisation-by-name")]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrganisationResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationsByCompaniesHouseName(string companiesHouseName)
    {
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNameAsync(companiesHouseName);
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
    [Route("organisation-by-reference-number")]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrganisationResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationByReferenceNumberAsync(string referenceNumber)
    {
        var organisationList = await _organisationService.GetOrganisationByReferenceNumber(referenceNumber);
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
    [Route("organisation-by-relationship")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisationRelationshipAsync(int parentId, int subsidiaryId)
    {
        var relationshipExists = await _organisationService.IsOrganisationInRelationship(parentId, subsidiaryId);

        if (relationshipExists)
        {
            return Ok(relationshipExists);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpPost]
    [Route("create-subsidiary-and-add-relationship")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateSubsidiaryAndRelationship(BulkOrganisationRequestModel request)
    {
        return await ExecuteCreateSubsidiaryAction(
        request.UserId,
        request.ParentOrganisationId,
        async () =>
        {
            var organisationRelationshipModel = new OrganisationRelationshipModel
            {
                FirstOrganisationId = _organisation.Id,
                OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
                OrganisationRegistrationTypeId = null,
                RelationFromDate = DateTime.UtcNow,
                LastUpdatedById = _user.Id,
                LastUpdatedByOrganisationId = _organisation.Id,
                JoinerDate = JoinerDateValueCheck.GetJoinerDate(request)
            };

            var result = await _organisationService.AddOrganisationAndOrganisationRelationshipsAsync(request.Subsidiary,
                                    organisationRelationshipModel, request.UserId);

            if (result.OrganisationRelationships == null)
            {
                return NoContent();
            }
            return Ok(result.ReferenceNumber);
        });
    }

    [HttpPost]
    [Route("add-subsidiary-relationship")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSubsidiaryRelationship(BulkSubsidiaryAddRequestModel request)
    {
        if (request == null) 
        {
            return NotFound();
        }

        return await ExecuteCreateSubsidiaryAction(request.UserId, request.ParentOrganisationExternalId, async () =>
        {
            var organisationRelationshipModel = new OrganisationRelationshipModel
            {
                FirstOrganisationId = _organisation.Id,
                OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
                OrganisationRegistrationTypeId = null,
                RelationFromDate = DateTime.UtcNow,
                LastUpdatedById = _user.Id,
                LastUpdatedByOrganisationId = _organisation.Id,
                JoinerDate = JoinerDateValueCheck.GetJoinerDate(request)
            };

            var childOrganisation = await _organisationService.GetOrganisationResponseByExternalId(request.ChildOrganisationExternalId);

            if (childOrganisation is null)
            {
                return ValidationProblem(
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "child organisation does not exist",
                    type: "child organisation/invalid-childOrganisationId");
            }

            organisationRelationshipModel.SecondOrganisationId = childOrganisation.Id;

            var result = await _organisationService.AddOrganisationRelationshipsAsync(organisationRelationshipModel, request.ParentOrganisationExternalId, request.UserId);

            return Ok(result != null ? childOrganisation!.ReferenceNumber : null);
        });
    }

    [HttpPost]
    [Route("update-subsidiary-relationship")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateSubsidiaryRelationship(BulkSubsidiaryUpdateRequestModel request)
    {
        return await ExecuteCreateSubsidiaryAction(request.UserId, request.ParentOrganisationExternalId, async () =>
        {
            var organisationRelationshipModel = new OrganisationRelationshipModel
            {
                FirstOrganisationId = _organisation.Id,
                OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
                JoinerDate = JoinerDateValueCheck.GetJoinerDate(request)
            };

            var childOrganisation = await _organisationService.GetOrganisationResponseByExternalId(request.ChildOrganisationExternalId);
            organisationRelationshipModel.SecondOrganisationId = childOrganisation.Id;
            var result = await _organisationService.UpdateOrganisationRelationshipsAsync(organisationRelationshipModel, request.ParentOrganisationExternalId, request.UserId);
            return Ok(result != null ? childOrganisation!.ReferenceNumber : null);
        });
    }

    [HttpPost]
    [Route("terminate-subsidiary-relationship")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TerminateSubsidiaryRelationship(BulkSubsidiaryTerminateRequestModel request)
    {
        return await ExecuteCreateSubsidiaryAction(request.UserId, request.ParentOrganisationExternalId, async () =>
        {
            var terminateRequest = new TerminateSubsidiaryModel
            {
                ParentExternalId = request.ParentOrganisationExternalId,
                ParentOrganisationId = _organisation.Id,
                ChildOrganisationId = int.Parse(request.ChildOrganisationId),
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

    private async Task<IActionResult> ExecuteCreateSubsidiaryAction(Guid userId, Guid organisationExternalId, Func<Task<IActionResult>> action)
    {
        _organisation = await _organisationService.GetOrganisationResponseByExternalId(organisationExternalId);

        if (_organisation is null)
        {
            ModelState.AddModelError(nameof(organisationExternalId),
                $"organisation '{organisationExternalId}' does not exist");
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
}