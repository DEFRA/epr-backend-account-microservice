using BackendAccountService.Api.Extensions;
using BackendAccountService.Api.HealthChecks;
using BackendAccountService.Api.Middlewares;
using BackendAccountService.Api.Validators;
using BackendAccountService.Core.Helpers;
using BackendAccountService.Data.Infrastructure;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetry()
    .RegisterWebComponents(builder.Configuration)
    .RegisterDataComponents(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddHealthChecks()
    .AddDbContextCheck<AccountsDbContext>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
});

builder.Services.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<PersonsDetailsRequestValidator>();
    fv.AutomaticValidationEnabled = false;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("RunMigration"))
{
	app.MigrateDbContext<AccountsDbContext>();
}

app.UseExceptionHandler("/error");
app.UseMiddleware<CustomExceptionHandlingMiddleware>();
app.MapControllers();

app.MapHealthChecks(
    builder.Configuration.GetValue<string>("HealthCheckPath"),
    HealthCheckOptionBuilder.Build()).AllowAnonymous();

app.Run();
