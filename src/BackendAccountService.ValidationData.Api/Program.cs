using BackendAccountService.Data.Infrastructure;
using BackendAccountService.ValidationData.Api.Config;
using BackendAccountService.ValidationData.Api.Extensions;
using BackendAccountService.ValidationData.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.ConfigureOptions();

builder.Services.AddScoped<IOrganisationDataService, OrganisationDataService>();
builder.Services.AddScoped<ICompanyDetailsDataService, CompanyDetailsDataService>();
builder.Services.AddScoped<ISubsidiaryDataService, SubsidiaryDataService>();

builder.Services.AddDbContext<AccountsDbContext>((sp, options) =>
{
    var dbConfig = sp.GetRequiredService<IOptions<AccountsDatabaseConfig>>().Value;
    options.UseSqlServer(dbConfig.ConnectionString);
});

await builder.Build().RunAsync();