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

const string WiktionaryUrl = "https://kaikki.org/dictionary/downloads/ru/ru-extract.jsonl.gz";

var source = args.FirstOrDefault() ?? await DownloadAsync(DownloadUrl);
var wiktionarySource = await DownloadAsync(WiktionaryUrl);

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
await using var wiktionary = WiktionaryReader.Open(wiktionarySource);
var count = await DictImporter.ImportAsync(db, MergeSources(FreeDictReader.ReadEntries(tei), WiktionaryReader.ReadEntries(wiktionary)));
Console.WriteLine($"Imported entries: {count} in {sw.Elapsed.TotalSeconds:F1} s");

// FreeDict entries are kept as they are; Wiktionary only adds headwords that FreeDict does not have.
static IEnumerable<Woerterbuch.Core.Entities.DictEntry> MergeSources(
    IEnumerable<Woerterbuch.Core.Entities.DictEntry> primary,
    IEnumerable<Woerterbuch.Core.Entities.DictEntry> secondary)
{
    var known = new HashSet<string>(StringComparer.Ordinal);
    foreach (var entry in primary)
    {
        known.Add(entry.De);
        yield return entry;
    }

    foreach (var entry in secondary)
    {
        if (!known.Contains(entry.De))
            yield return entry;
    }
}

static async Task<string> DownloadAsync(string url)
{
    var path = Path.Combine(WoerterbuchDb.DataDirectory(), Path.GetFileName(new Uri(url).LocalPath));
    if (File.Exists(path))
    {
        Console.WriteLine($"Already downloaded: {path}");
        return path;
    }

    Console.WriteLine($"Downloading {url}");
    using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
    await using (var response = await http.GetStreamAsync(url))
    await using (var file = File.Create(path + ".part"))
        await response.CopyToAsync(file);
    File.Move(path + ".part", path, overwrite: true);
    return path;
}
