using BackendAccountService.ValidationData.Api.Extensions;
using BackendAccountService.ValidationData.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BackendAccountService.ValidationData.Api.Models;

namespace BackendAccountService.ValidationData.Api;

public class OrganisationFunctions : FunctionsBase
{
    private readonly IOrganisationDataService _organisationService;
    private readonly ILogger<OrganisationFunctions> _logger;

    public OrganisationFunctions(
        IOrganisationDataService organisationService,
        ILogger<OrganisationFunctions> logger)
    {
        _organisationService = organisationService;
        _logger = logger;
    }

    [FunctionName("GetOrganisation")]
    public async Task<IActionResult> GetOrganisationAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            nameof(HttpMethod.Get),
            Route = "organisation/{organisationId:guid}")]
        HttpRequest req,
        Guid organisationId)
    {
        _logger.LogEnter();

        try
        {
            var result =
                await _organisationService.GetOrganisationByExternalId(
                    organisationId);

            if (result is null)
            {
                return Problem("Organisation not found", statusCode: StatusCodes.Status404NotFound);
            }

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("{ExceptionMessage}", ex.Message);
            return Problem("Unhandled exception", ex.GetType().Name, ex.Message);
        }
        finally
        {
            _logger.LogExit();
        }
    }

    [FunctionName("GetOrganisationMembers")]
    public async Task<IActionResult> GetOrganisationMembersAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            nameof(HttpMethod.Get),
            Route = "organisation/{organisationId:guid}/members/{complianceSchemeId:guid}")]
        HttpRequest req,
        Guid organisationId,
        Guid complianceSchemeId)
    {
        _logger.LogEnter();

        try
        {
            var result =
                await _organisationService.GetOrganisationMembersByComplianceSchemeId(
                    organisationId, complianceSchemeId);

            if (result is null)
            {
                return Problem("Compliance scheme details not found", statusCode: StatusCodes.Status404NotFound);
            }

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("{ExceptionMessage}", ex.Message);
            return Problem("Unhandled exception", ex.GetType().Name, ex.Message);
        }
        finally
        {
            _logger.LogExit();
        }
    }

    [FunctionName("GetExistingOrganisations")]
    public async Task<IActionResult> GetExistingOrganisationsAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            nameof(HttpMethod.Post),
            Route = "organisations")]
        HttpRequest req)
    {
        _logger.LogEnter();

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var requestData = JsonSerializer.Deserialize<OrganisationsRequest>(requestBody);

            if (requestData.ReferenceNumbers == null)
            {
                return Problem("Invalid ReferenceNumbers property in request body", statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await _organisationService.GetExistingOrganisationsByReferenceNumber(requestData.ReferenceNumbers);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("{ExceptionMessage}", ex.Message);
            return Problem("Unhandled exception", ex.GetType().Name, ex.Message);
        }
        finally
        {
            _logger.LogExit();
        }
    }
}