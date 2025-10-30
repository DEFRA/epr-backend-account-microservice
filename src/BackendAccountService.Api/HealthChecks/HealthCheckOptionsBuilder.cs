using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Api.HealthChecks;

[ExcludeFromCodeCoverage]
public static class HealthCheckOptionBuilder
{
    public static HealthCheckOptions Build() => new()
    {
        AllowCachingResponses = false,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK
        }
    };
}