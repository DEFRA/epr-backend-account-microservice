namespace BackendAccountService.Api.Controllers;

using Core.Services;
using Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

public class ApiControllerBase : ControllerBase
{
    private readonly string _baseProblemTypePath;

    public ApiControllerBase(IOptions<ApiConfig> baseApiConfigOptions)
    {
        _baseProblemTypePath= baseApiConfigOptions.Value.BaseProblemTypePath;
    }

    // ASP.NET Core 10 added a 6-arg ControllerBase.Problem (and 7-arg ValidationProblem)
    // overload with an IDictionary<string, object?>? extensions parameter. The new overload
    // has all-optional args, so calls that omit `extensions` are ambiguous with the older
    // overload — and C# has no tiebreaker for "fewer omitted optionals". Custom methods
    // named TypedProblem/TypedValidationProblem sidestep the ambiguity entirely.

    [NonAction]
    public override ActionResult ValidationProblem()
    {
        return base.ValidationProblem(type: $"{_baseProblemTypePath}validation".ToLower());
    }

    [NonAction]
    protected ActionResult TypedValidationProblem(
        string? detail = null,
        string? instance = null,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        [ActionResultObjectValue] ModelStateDictionary? modelStateDictionary = null)
    {
        modelStateDictionary ??= ModelState;
        type = type ?? $"{_baseProblemTypePath}validation";

        ValidationProblemDetails? validationProblem;
        if (ProblemDetailsFactory == null)
        {
            // ProblemDetailsFactory may be null in unit testing scenarios. Improvise to make this more testable.
            validationProblem = new ValidationProblemDetails(modelStateDictionary)
            {
                Detail = detail,
                Instance = instance,
                Status = statusCode,
                Title = title,
                Type = type,
            };
        }
        else
        {
            validationProblem = ProblemDetailsFactory?.CreateValidationProblemDetails(
                HttpContext,
                modelStateDictionary,
                statusCode: statusCode,
                title: title,
                type: type,
                detail: detail,
                instance: instance);
        }

        if (validationProblem is { Status: 400 })
        {
            // For compatibility with 2.x, continue producing BadRequestObjectResult instances if the status code is 400.
            return new BadRequestObjectResult(validationProblem);
        }

        return new ObjectResult(validationProblem)
        {
            StatusCode = validationProblem?.Status
        };
    }

    [NonAction]
    protected ObjectResult TypedProblem(
        string? detail = null,
        string? instance = null,
        int? statusCode = null,
        string? title = null,
        string? type = null)
    {
        return base.Problem(detail, instance, statusCode, title, $"{_baseProblemTypePath}{type}".ToLower());
    }

    [NonAction]
    protected ObjectResult TypedProblem(
        Exception type,
        string? detail = null,
        string? instance = null,
        int? statusCode = null,
        string? title = null)
    {
        var exceptionName = type.GetType().Name;
        title = title ?? exceptionName;

        return base.Problem(
            detail,
            instance,
            statusCode,
            title,
            $"{_baseProblemTypePath}{exceptionName}".ToLower());
    }
}
