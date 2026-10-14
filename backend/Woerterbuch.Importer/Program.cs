// Import of the FreeDict deu-rus dictionary (CC BY-SA 3.0, https://freedict.org, WikDict/Wiktionary data).
// Usage:
//   dotnet run --project backend/Woerterbuch.Importer                 — download and import
//   dotnet run --project backend/Woerterbuch.Importer -- path/to.tei  — import from a local .tei or .tar.xz

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Woerterbuch.Core;
using Woerterbuch.Core.Backup;
using Woerterbuch.Importer;

const string DownloadUrl =
    "https://download.freedict.org/dictionaries/deu-rus/2025.11.23/freedict-deu-rus-2025.11.23.src.tar.xz";

var source = args.FirstOrDefault() ?? await DownloadAsync();

var options = new DbContextOptionsBuilder<WoerterbuchDbContext>()
    .UseSqlite(Environment.GetEnvironmentVariable($"ConnectionStrings__{WoerterbuchDb.ConnectionName}")
               ?? WoerterbuchDb.DefaultConnectionString())
    .Options;

await using var db = new WoerterbuchDbContext(options);
await db.Database.MigrateAsync();

// The import only changes the dictionary, but backing up own words first does no harm.
if (await WordBackup.WriteAutoBackupAsync(db, WoerterbuchDb.BackupDirectory()) is { } backup)
    Console.WriteLine($"Word backup: {backup}");

var sw = Stopwatch.StartNew();
await using var tei = FreeDictReader.OpenTei(source);
var count = await DictImporter.ImportAsync(db, FreeDictReader.ReadEntries(tei));
Console.WriteLine($"Imported entries: {count} in {sw.Elapsed.TotalSeconds:F1} s");

static async Task<string> DownloadAsync()
{
    var path = Path.Combine(WoerterbuchDb.DataDirectory(), Path.GetFileName(new Uri(DownloadUrl).LocalPath));
    if (File.Exists(path))
    {
        Console.WriteLine($"Already downloaded: {path}");
        return path;
    }

    Console.WriteLine($"Downloading {DownloadUrl}");
    using var http = new HttpClient();
    await using (var response = await http.GetStreamAsync(DownloadUrl))
    await using (var file = File.Create(path + ".part"))
        await response.CopyToAsync(file);
    File.Move(path + ".part", path, overwrite: true);
    return path;
}
