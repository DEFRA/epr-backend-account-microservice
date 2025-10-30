using Microsoft.AspNetCore.Mvc;

namespace BackendAccountService.ValidationData.Api;

public abstract class FunctionsBase
{
    protected static ObjectResult Problem(
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