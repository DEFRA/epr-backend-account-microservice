using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BackendAccountService.Api.Extensions;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
public static class ServiceProviderExtensions
{
    private const string baseProblemTypePath = "ApiConfig:BaseProblemTypePath";
    public static IServiceCollection RegisterWebComponents(this IServiceCollection services, IConfiguration configuration)
    {
        AddControllers(services, configuration);
        ConfigureOptions(services, configuration);
        RegisterServices(services);

        return services;
    }

    public static IServiceCollection RegisterDataComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AccountsDbContext>(options =>
            options
                .UseSqlServer(configuration.GetConnectionString("AccountsDatabase"))
                .UseRecompileExtensions());

        return services;
    }

    private static void AddControllers(IServiceCollection services, IConfiguration configuration)
    {
        var baseProblemPath = configuration.GetValue<string>(baseProblemTypePath);

        services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(options =>
            {
                options.ClientErrorMapping[StatusCodes.Status400BadRequest].Link =
                    $"{baseProblemPath}validation";

                options.ClientErrorMapping[StatusCodes.Status409Conflict].Link =
                    $"{baseProblemPath}conflict";

                options.ClientErrorMapping[StatusCodes.Status404NotFound].Link =
                    $"{baseProblemPath}not-found";
            });
    }

    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiConfig>(configuration.GetSection(nameof(ApiConfig)));
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IComplianceSchemeService, ComplianceSchemeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrganisationService, OrganisationService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IAccountManagementService, AccountManagementService>();
        services.AddScoped<ILocalAuthorityService, LocalAuthorityService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IEnrolmentsService, EnrolmentsService>();
        services.AddScoped<IRegulatorService, RegulatorService>();
        services.AddScoped<INotificationsService, NotificationsService>();
        services.AddScoped<IRegulatorOrganisationService, RegulatorOrganisationService>();
        services.AddScoped<IReprocessorExporterService, ReprocessorExporterService>();
        services.AddScoped<IReprocessorExporterRepository, ReprocessorExporterRepository>();
        services.AddScoped<IReExEnrolmentMaps, ReExEnrolmentMaps>();
    }
}