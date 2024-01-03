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

namespace BackendAccountService.ValidationData.Api;

public class GetOrganisationValidationFunction
{
    private readonly IOrganisationDataService _organisationService;
    private readonly ILogger<GetOrganisationValidationFunction> _logger;

    public GetOrganisationValidationFunction(
        IOrganisationDataService organisationService,
        ILogger<GetOrganisationValidationFunction> logger)
    {
        _organisationService = organisationService;
        _logger = logger;
    }

    [FunctionName("GetOrganisationValidationFunction")]
    public async Task<IActionResult> RunAsync(
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
            return Problem("Unhandled exception", ex.GetType().Name,ex.Message);
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