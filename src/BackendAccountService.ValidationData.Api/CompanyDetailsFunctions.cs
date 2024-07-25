using BackendAccountService.ValidationData.Api.Extensions;
using BackendAccountService.ValidationData.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using BackendAccountService.ValidationData.Api.Models;
using System.IO;
using System.Text.Json;

namespace BackendAccountService.ValidationData.Api;

public class CompanyDetailsFunctions
{
    private readonly ICompanyDetailsDataService _companyDetailsService;
    private readonly ILogger<CompanyDetailsFunctions> _logger;

    public CompanyDetailsFunctions(
        ICompanyDetailsDataService companyDetailsService,
        ILogger<CompanyDetailsFunctions> logger)
    {
        _companyDetailsService = companyDetailsService;
        _logger = logger;
    }

    [FunctionName("GetCompanyDetails")]
    public async Task<IActionResult> GetCompanyDetailsAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            nameof(HttpMethod.Get),
            Route = "company-details/{organisationId}")]
        HttpRequest req,
        string organisationId)
    {
        _logger.LogEnter();

        try
        {
            var organisations = await _companyDetailsService.GetCompanyDetailsByOrganisationReferenceNumber(organisationId);

            if (organisations is null || organisationId is null)
            {
                return Problem("Organisation not found", statusCode: StatusCodes.Status404NotFound);
            }

            return new OkObjectResult(organisations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
            return Problem("Unhandled exception", ex.GetType().Name, ex.Message);
        }
        finally
        {
            _logger.LogExit();
        }
    }

    [FunctionName("GetCompanyDetailsByProducer")]
    public async Task<IActionResult> GetCompanyDetailsByProducerAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            nameof(HttpMethod.Get),
            Route = "company-details-by-producer/{producerOrganisationId}")]
        HttpRequest req,
        string producerOrganisationId)
    {
        _logger.LogEnter();

        try
        {
            if (!Guid.TryParse(producerOrganisationId, out var organisationExternalId))
            {
                return Problem("Check that the organisation ID is correct", statusCode: StatusCodes.Status404NotFound);
            }

            var organisations = await _companyDetailsService.GetCompanyDetailsByOrganisationExternalId(organisationExternalId);

            if (organisations is null)
            {
                return Problem("Organisation not found", statusCode: StatusCodes.Status404NotFound);
            }

            return new OkObjectResult(organisations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
            return Problem("Unhandled exception", ex.GetType().Name, ex.Message);
        }
        finally
        {
            _logger.LogExit();
        }
    }

    [FunctionName("GetCompanyDetailsByComplianceSchemeId")]
    public async Task<IActionResult> GetCompanyDetailsByComplianceSchemeIdAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            nameof(HttpMethod.Get),
            Route = "company-details/{organisationId}/compliance-scheme/{complianceSchemeId}")]
        HttpRequest req,
        string organisationId,
        Guid? complianceSchemeId)
    {
        _logger.LogEnter();

        try
        {
            var organisations = await _companyDetailsService.GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(organisationId, complianceSchemeId);

            if (organisations is null || organisationId is null)
            {
                return Problem("Organisation not found", statusCode: StatusCodes.Status404NotFound);
            }

            return new OkObjectResult(organisations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
            return Problem("Unhandled exception", ex.GetType().Name, ex.Message);
        }
        finally
        {
            _logger.LogExit();
        }
    }

    [FunctionName("GetAllProducersCompanyDetails")]
    public async Task<IActionResult> GetAllProducersCompanyDetailsAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            nameof(HttpMethod.Post),
            Route = "company-details")]
        HttpRequest req)
    {
        _logger.LogEnter();

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var referenceNumbers = JsonSerializer.Deserialize<OrganisationReferencesRequest>(requestBody);

            if (referenceNumbers == null)
            {
                return Problem("Invalid ReferenceNumbers property in request body", statusCode: StatusCodes.Status400BadRequest);
            }

            var organisations = await _companyDetailsService.GetAllProducersCompanyDetailsAsProducer(referenceNumbers);

            if (organisations is null)
            {
                return Problem("Organisations not found", statusCode: StatusCodes.Status404NotFound);
            }

            return new OkObjectResult(organisations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
            return Problem("Unhandled exception", ex.GetType().Name, ex.Message);
        }
        finally
        {
            _logger.LogExit();
        }
    }

    private static ObjectResult Problem(
        string title,
        string? type = null,
        string? detail = null,
        int statusCode = 500)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Detail = detail,
            Type = type,
            Title = title
        };

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status
        };
    }
}