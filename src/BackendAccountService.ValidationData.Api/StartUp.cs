using BackendAccountService.Data.Infrastructure;
using BackendAccountService.ValidationData.Api;
using BackendAccountService.ValidationData.Api.Config;
using BackendAccountService.ValidationData.Api.Extensions;
using BackendAccountService.ValidationData.Api.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: FunctionsStartup(typeof(StartUp))]

namespace BackendAccountService.ValidationData.Api;

[ExcludeFromCodeCoverage]
public class StartUp : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        try
        {
            var services = builder.Services;

            services.ConfigureOptions();

            var serviceProvider = services.BuildServiceProvider();
            var accountDatabaseOptions = serviceProvider.GetRequiredService<IOptions<AccountsDatabaseConfig>>().Value;
            services.AddScoped<IOrganisationDataService, OrganisationDataService>();
            services.AddScoped<ICompanyDetailsDataService, CompanyDetailsDataService>();
            services.AddDbContext<AccountsDbContext>(options =>
                options.UseSqlServer(accountDatabaseOptions.ConnectionString));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}