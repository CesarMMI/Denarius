using Denarius.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Denarius.Api.Extensions;

public static class DatabaseMigrationExtensions
{
    public static WebApplication ApplyPendingMigrations(this WebApplication app)
    {
        if (!app.Configuration.GetValue("RUN_MIGRATIONS_ON_STARTUP", defaultValue: false))
            return app;

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DenariusDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        const int maxAttempts = 5;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                logger.LogInformation("Applying database migrations (attempt {Attempt}/{Max})...", attempt, maxAttempts);
                dbContext.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully.");
                break;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Migration attempt {Attempt} failed. Retrying in 5s...", attempt);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        return app;
    }
}
