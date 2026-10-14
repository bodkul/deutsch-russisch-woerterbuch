using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Woerterbuch.Api;
using Woerterbuch.Core;
using Woerterbuch.Core.Backup;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddWoerterbuchDb(builder.Configuration);
builder.Services.AddOpenApi();
// No reading numbers from strings, so OpenAPI describes numbers as number rather than number | string.
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
builder.Services.AddValidation();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Local app: apply migrations on startup so the database is always up to date.
// Back up own words before and after migrations (identical copies are not duplicated).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WoerterbuchDbContext>();
    await BackupWordsAsync(db, "before migrations");
    db.Database.Migrate();
    await BackupWordsAsync(db, "after migrations");

    if (!db.DictEntries.Any())
        app.Logger.LogWarning("Dictionary is empty. Run the import: the \"importer\" resource in the Aspire dashboard or dotnet run --project backend/Woerterbuch.Importer");
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

var api = app.MapGroup("/api");
api.MapSearchEndpoints();
api.MapWordEndpoints();

app.Run();

async Task BackupWordsAsync(WoerterbuchDbContext db, string stage)
{
    try
    {
        var path = await WordBackup.WriteAutoBackupAsync(db, WoerterbuchDb.BackupDirectory());
        if (path is not null)
            app.Logger.LogInformation("Word backup ({Stage}): {Path}", stage, path);
    }
    catch (Exception ex)
    {
        // E.g. before the first migration there are no tables yet; that is no reason not to start.
        app.Logger.LogWarning(ex, "Word backup ({Stage}) failed", stage);
    }
}
