using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Woerterbuch.Core;
using Woerterbuch.Core.Backup;
using Woerterbuch.Core.Entities;
using Woerterbuch.Core.Search;

namespace Woerterbuch.Api;

/// <summary>Word form data (create and edit).</summary>
public record WordInput(
    [property: Required, StringLength(200)] string De,
    [property: AllowedValues("der", "die", "das", null)] string? Article,
    [property: StringLength(200)] string? Plural,
    [property: StringLength(50)] string? Pos,
    [property: Required, StringLength(2000)] string Ru,
    List<string>? Tags,
    int? DictEntryId);

public static class WordEndpoints
{
    public static RouteGroupBuilder MapWordEndpoints(this RouteGroupBuilder api)
    {
        var words = api.MapGroup("/words");

        words.MapGet("/", async (string? tag, WoerterbuchDbContext db, CancellationToken ct) =>
        {
            var query = db.Words.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(w => w.Tags.Contains(tag));

            var list = await query.OrderByDescending(w => w.CreatedAt).ToListAsync(ct);
            return list.Select(WordDto.From).ToList();
        })
        .WithName("ListWords");

        words.MapGet("/{id:int}", async Task<Results<Ok<WordDto>, NotFound>> (int id, WoerterbuchDbContext db, CancellationToken ct) =>
            await db.Words.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct) is { } w
                ? TypedResults.Ok(WordDto.From(w))
                : TypedResults.NotFound())
        .WithName("GetWord");

        words.MapPost("/", async Task<Created<WordDto>> (WordInput input, WoerterbuchDbContext db, CancellationToken ct) =>
        {
            var word = new Word { De = "", Ru = "" };
            Apply(word, input);
            db.Words.Add(word);
            await db.SaveChangesAsync(ct);
            return TypedResults.Created($"/api/words/{word.Id}", WordDto.From(word));
        })
        .WithName("CreateWord");

        words.MapPut("/{id:int}", async Task<Results<Ok<WordDto>, NotFound>> (int id, WordInput input, WoerterbuchDbContext db, CancellationToken ct) =>
        {
            var word = await db.Words.FindAsync([id], ct);
            if (word is null)
                return TypedResults.NotFound();

            Apply(word, input);
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(WordDto.From(word));
        })
        .WithName("UpdateWord");

        words.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (int id, WoerterbuchDbContext db, CancellationToken ct) =>
            await db.Words.Where(w => w.Id == id).ExecuteDeleteAsync(ct) > 0
                ? TypedResults.NoContent()
                : TypedResults.NotFound())
        .WithName("DeleteWord");

        words.MapGet("/export", async (WoerterbuchDbContext db, CancellationToken ct) =>
        {
            var file = await WordBackup.ExportAsync(db, ct);
            var json = JsonSerializer.SerializeToUtf8Bytes(file, WordBackup.JsonOptions);
            return TypedResults.File(json, "application/json", $"meine-woerter-{DateTime.Now:yyyy-MM-dd}.json");
        })
        .WithName("ExportWords")
        .WithSummary("Download personal words as a single JSON file");

        words.MapPost("/import", async Task<Results<Ok<ImportResult>, ProblemHttpResult>> (WordExportFile file, WoerterbuchDbContext db, CancellationToken ct) =>
        {
            if (file.Format != WordExportFile.FormatName || file.Words is null)
                return TypedResults.Problem("This is not a word export file", statusCode: StatusCodes.Status400BadRequest);

            // Back up the current state before importing.
            await WordBackup.WriteAutoBackupAsync(db, WoerterbuchDb.BackupDirectory(), ct: ct);
            return TypedResults.Ok(await WordBackup.ImportAsync(db, file, ct));
        })
        .WithName("ImportWords")
        .WithSummary("Add words from an export file (deletes nothing)");

        api.MapGet("/tags",async (WoerterbuchDbContext db, CancellationToken ct) =>
        {
            // The personal list is small, so collecting tags in memory is simplest.
            var all = await db.Words.AsNoTracking().Select(w => w.Tags).ToListAsync(ct);
            return all.SelectMany(t => t)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        })
        .WithName("ListTags");

        return api;
    }

    private static void Apply(Word word, WordInput input)
    {
        word.De = input.De.Trim();
        word.Article = Clean(input.Article);
        word.Plural = Clean(input.Plural);
        word.Pos = Clean(input.Pos);
        word.Ru = input.Ru.Trim();
        word.Tags = (input.Tags ?? [])
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        word.DictEntryId = input.DictEntryId;
    }

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
