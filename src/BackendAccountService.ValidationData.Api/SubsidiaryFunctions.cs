using BackendAccountService.ValidationData.Api.Extensions;
using BackendAccountService.ValidationData.Api.Models;
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

namespace BackendAccountService.ValidationData.Api;

public class SubsidiaryFunctions : FunctionsBase
{
    private readonly ISubsidiaryDataService _subsidiaryDataService;
    private readonly ILogger<SubsidiaryFunctions> _logger;

    public SubsidiaryFunctions(ISubsidiaryDataService subsidiaryDataService, ILogger<SubsidiaryFunctions> logger)
    {
        _subsidiaryDataService = subsidiaryDataService;
        _logger = logger;
    }

    [FunctionName("GetSubsidiaryDetails")]
    public async Task<IActionResult> GetSubsidiaryDetailsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Post), Route = "subsidiary-details")] HttpRequest req)
    {
        _logger.LogEnter();

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var subsidiaryDetailsRequest = JsonSerializer.Deserialize<SubsidiaryDetailsRequest>(requestBody);

            var subsidiaryDetailResponse = await _subsidiaryDataService.GetSubsidiaryDetails(subsidiaryDetailsRequest);

            if (subsidiaryDetailResponse is null)
            {
                return Problem("Organisation not found", statusCode: StatusCodes.Status404NotFound);
            }

            return new OkObjectResult(subsidiaryDetailResponse);
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
}