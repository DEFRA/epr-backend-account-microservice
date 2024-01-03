﻿using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Api.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplication MigrateDbContext<TContext>(this WebApplication app) 
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var context = services.GetService<TContext>();

        try
        {
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            context.Database.Migrate();

            logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
        }

        return app;
    }
}
