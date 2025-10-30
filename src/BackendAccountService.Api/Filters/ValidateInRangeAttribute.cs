using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Api.Filters;

[ExcludeFromCodeCoverage]
public class ValidateInRangeAttribute(string parameterName, int min, int max) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if(context.ActionArguments.TryGetValue(parameterName, out var value) && value is int intValue && (intValue < min || intValue > max))
                context.Result = new BadRequestObjectResult(
                    $"{parameterName} must be between {min} and {max}"
                    );

        base.OnActionExecuting(context);
    }
}
