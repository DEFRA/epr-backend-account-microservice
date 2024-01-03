using BackendAccountService.Api.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ApiControllerBase
    {
        public ErrorsController(IOptions<ApiConfig> baseApiConfigOptions) : base(baseApiConfigOptions)
        {
        }

        [Route("/error")]
        public IActionResult HandleError()
        {
            IExceptionHandlerFeature? exceptionHandlerFeature =
                HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (exceptionHandlerFeature == null)
            {
                return Problem(
                    type: "internalservererror",
                    title: "Unhandled exception",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            return Problem(
                exceptionHandlerFeature.Error,
                statusCode: StatusCodes.Status500InternalServerError,
                detail: exceptionHandlerFeature.Error.Message);
        }
    }
}

