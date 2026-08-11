using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace API.Extensions;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations at startup when Database:AutoMigrate is enabled.
    /// This is an opt-in production convenience — the recommended approach is pre-deployment scripts.
    /// </summary>
    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        var autoMigrate = app.Configuration.GetValue<bool>("Database:AutoMigrate");

        if (!autoMigrate)
            return app;

        var envName = app.Configuration["Environment"]
                   ?? app.Configuration["ASPNETCORE_ENVIRONMENT"]
                   ?? "Production";

        Log.Information("Database:AutoMigrate is enabled. Checking for pending migrations...");

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Count == 0)
            {
                Log.Information("Database is up to date. No pending migrations.");
                return app;
            }

            Log.Information("Found {Count} pending migration(s): {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));

            dbContext.Database.Migrate();

            Log.Information("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database migration failed. Application cannot start.");
            throw;
        }

        return app;
    }
}
