namespace BackendAccountService.Api.Helpers;

using System.Net;
using Core.Models.Result;
using Microsoft.AspNetCore.Mvc;

public static class ResultHelper
{
    public static IActionResult BuildErrorResponse<T>(this Result<T> result) where T : class
    {
        switch (result.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                return new BadRequestObjectResult(result.ErrorMessage);
            case HttpStatusCode.NotFound:
                return new NotFoundObjectResult(result.ErrorMessage);
            default:
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
        }
    }
    
    public static IActionResult BuildErrorResponse(this Result result)
    {
        switch (result.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                return new BadRequestObjectResult(result.ErrorMessage);
            case HttpStatusCode.NotFound:
                return new NotFoundObjectResult(result.ErrorMessage);
            default:
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
        }
    }
}