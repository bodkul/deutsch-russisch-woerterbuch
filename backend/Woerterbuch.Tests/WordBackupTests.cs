using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Woerterbuch.Core;
using Woerterbuch.Core.Backup;
using Woerterbuch.Core.Entities;

namespace Woerterbuch.Tests;

public sealed class WordBackupTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly string _backupDir = Path.Combine(Path.GetTempPath(), "woerterbuch-tests", Guid.NewGuid().ToString("N"));
    private WoerterbuchDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _db = new WoerterbuchDbContext(new DbContextOptionsBuilder<WoerterbuchDbContext>().UseSqlite(_connection).Options);
        await _db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
        if (Directory.Exists(_backupDir))
            Directory.Delete(_backupDir, recursive: true);
    }

    private async Task AddWordAsync(string de, string? article, string ru)
    {
        _db.Words.Add(new Word { De = de, Article = article, Ru = ru, Tags = ["B2"] });
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task ExportThenImport_IntoEmptyDb_RestoresWords()
    {
        await AddWordAsync("Prüfung", "die", "экзамен");
        await AddWordAsync("bestehen", null, "сдать");
        var file = await WordBackup.ExportAsync(_db);

        await _db.Words.ExecuteDeleteAsync();
        var result = await WordBackup.ImportAsync(_db, file);

        Assert.Equal(new ImportResult(2, 0), result);
        var restored = (await _db.Words.ToListAsync()).OrderBy(w => w.De, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(["bestehen", "Prüfung"], restored.Select(w => w.De));
        Assert.Equal(["B2"], restored[1].Tags);
    }

    [Fact]
    public async Task Import_SkipsExistingAndInvalid_AndNeverDeletes()
    {
        await AddWordAsync("Haus", "das", "дом");
        await AddWordAsync("Tisch", "der", "стол");

        var file = new WordExportFile(WordExportFile.FormatName, 1, DateTime.UtcNow,
        [
            new("haus", "das", null, null, "дом", null),        // already exists (case-insensitive)
            new("Haus", "der", null, null, "?", null),          // different article, different word
            new("Prüfung", "die", "Prüfungen", "n", "экзамен", ["B2"]),
            new("Prüfung", "die", null, null, "экзамен", null), // duplicate within the file
            new("", null, null, null, "пусто", null),           // invalid
        ]);

        var result = await WordBackup.ImportAsync(_db, file);

        Assert.Equal(new ImportResult(2, 3), result);
        Assert.Equal(4, await _db.Words.CountAsync());
    }

    [Fact]
    public async Task AutoBackup_SkipsEmptyAndUnchanged_AndKeepsLimit()
    {
        Assert.Null(await WordBackup.WriteAutoBackupAsync(_db, _backupDir));

        await AddWordAsync("Haus", "das", "дом");
        Assert.NotNull(await WordBackup.WriteAutoBackupAsync(_db, _backupDir));
        Assert.Null(await WordBackup.WriteAutoBackupAsync(_db, _backupDir));

        for (var i = 0; i < 4; i++)
        {
            await AddWordAsync($"Wort{i}", null, "слово");
            await Task.Delay(1100); // file names have one-second precision
            Assert.NotNull(await WordBackup.WriteAutoBackupAsync(_db, _backupDir, keep: 3));
        }

        var files = Directory.GetFiles(_backupDir).Order().ToList();
        Assert.Equal(3, files.Count);
        Assert.Contains("Wort3", await File.ReadAllTextAsync(files[^1]));
    }
}
