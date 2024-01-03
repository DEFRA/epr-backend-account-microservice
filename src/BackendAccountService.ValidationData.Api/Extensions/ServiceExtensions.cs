using System.Diagnostics.CodeAnalysis;
using BackendAccountService.ValidationData.Api.Config;
using Microsoft.Extensions.DependencyInjection;

namespace BackendAccountService.ValidationData.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services)
    {
        services.ConfigureSection<AccountsDatabaseConfig>(AccountsDatabaseConfig.Section);
        return services;
    }
}