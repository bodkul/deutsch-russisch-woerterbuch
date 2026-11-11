using Woerterbuch.Core;
using Woerterbuch.Core.Backup;

namespace Woerterbuch.Api.Startup;

public static class StartupBackup
{
    public static async Task BackupWordsAsync(WoerterbuchDbContext db, ILogger logger, string stage)
    {
        try
        {
            var path = await WordBackup.WriteAutoBackupAsync(db, WoerterbuchDb.BackupDirectory());
            if (path is not null && logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Word backup ({Stage}): {Path}", stage, path);
        }
        catch (Exception ex)
        {
            // E.g. before the first migration there are no tables yet; that is no reason not to start.
            logger.LogWarning(ex, "Word backup ({Stage}) failed", stage);
        }
    }
}
