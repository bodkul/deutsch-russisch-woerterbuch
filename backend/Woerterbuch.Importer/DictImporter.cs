using Microsoft.EntityFrameworkCore;
using Woerterbuch.Core;
using Woerterbuch.Core.Entities;

namespace Woerterbuch.Importer;

public static class DictImporter
{
    private const int BatchSize = 2000;

    /// <summary>
    /// Fully replaces the dictionary. Links from personal words to dictionary entries
    /// are restored by the (word, part of speech) pair.
    /// </summary>
    public static async Task<int> ImportAsync(WoerterbuchDbContext db, IEnumerable<DictEntry> entries)
    {
        var links = await db.Words
            .Where(w => w.DictEntry != null)
            .Select(w => new { WordId = w.Id, w.DictEntry!.De, w.DictEntry.Pos })
            .ToListAsync();

        await using var tx = await db.Database.BeginTransactionAsync();
        await db.DictEntries.ExecuteDeleteAsync();

        var count = 0;
        foreach (var batch in entries.Chunk(BatchSize))
        {
            db.DictEntries.AddRange(batch);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            count += batch.Length;
            Console.WriteLine($"  … {count}");
        }

        foreach (var link in links)
        {
            var newId = await db.DictEntries
                .Where(e => e.De == link.De && e.Pos == link.Pos)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();
            await db.Words
                .Where(w => w.Id == link.WordId)
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.DictEntryId, newId));
        }

        await tx.CommitAsync();
        return count;
    }
}
