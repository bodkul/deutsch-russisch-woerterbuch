using Microsoft.EntityFrameworkCore;
using Woerterbuch.Core;

namespace Woerterbuch.Api.Startup;

public static class DatabaseStartup
{
    /// <summary>
    /// Local app: applies migrations on startup so the database is always up to date.
    /// Backs up own words before and after migrations (identical copies are not duplicated).
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoerterbuchDbContext>();

        await StartupBackup.BackupWordsAsync(db, app.Logger, "before migrations");
        await db.Database.MigrateAsync();
        await StartupBackup.BackupWordsAsync(db, app.Logger, "after migrations");

        if (!await db.DictEntries.AnyAsync())
            app.Logger.LogWarning("Dictionary is empty. Run the import: the \"importer\" resource in the Aspire dashboard or dotnet run --project backend/Woerterbuch.Importer");
    }
}
