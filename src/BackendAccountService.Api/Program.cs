using BackendAccountService.Api.Extensions;
using BackendAccountService.Api.HealthChecks;
using BackendAccountService.Data.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetry()
    .RegisterWebComponents(builder.Configuration)
    .RegisterDataComponents(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddHealthChecks()
    .AddDbContextCheck<AccountsDbContext>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MigrateDbContext<AccountsDbContext>();
}

app.UseExceptionHandler("/error");
app.MapControllers();

app.MapHealthChecks(
    builder.Configuration.GetValue<string>("HealthCheckPath"),
    HealthCheckOptionBuilder.Build()).AllowAnonymous();

app.Run();
