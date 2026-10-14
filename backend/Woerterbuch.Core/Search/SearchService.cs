using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Woerterbuch.Core.Entities;

namespace Woerterbuch.Core.Search;

/// <summary>
/// DE↔RU search over the personal list and the dictionary. FTS5 picks candidates by word prefixes,
/// then they are ranked: exact match → word start → the rest.
/// </summary>
public partial class SearchService(WoerterbuchDbContext db)
{
    private const int CandidateLimit = 200;

    public async Task<SearchResult> SearchAsync(string query, int limit = 30, CancellationToken ct = default)
    {
        query = query.Trim();
        var isRu = TextNormalizer.ContainsCyrillic(query);
        var direction = isRu ? "ru" : "de";
        var normalized = isRu ? TextNormalizer.NormalizeRu(query) : TextNormalizer.NormalizeDe(query);

        var match = BuildMatch(normalized, column: isRu ? "ru" : "de");
        if (match is null)
            return new SearchResult(query, direction, [], []);

        var wordIds = await MatchWordIdsAsync(match, ct);
        var dictIds = await MatchDictIdsAsync(match, ct);

        var words = await db.Words.AsNoTracking().Where(w => wordIds.Contains(w.Id)).ToListAsync(ct);
        var entries = await db.DictEntries.AsNoTracking().Where(e => dictIds.Contains(e.Id)).ToListAsync(ct);

        var rankedWords = words
            .OrderBy(w => isRu ? RankRu(w.Ru, normalized) : RankDe(normalized, w.De, w.Plural))
            .ThenBy(w => w.De.Length)
            .ThenBy(w => w.De, StringComparer.CurrentCultureIgnoreCase)
            .Take(limit)
            .Select(WordDto.From)
            .ToList();

        var rankedEntries = entries
            .OrderBy(e => isRu ? RankRu(e.Ru, normalized) : RankDe(normalized, e.De))
            .ThenBy(e => e.De.Length)
            .ThenBy(e => e.De, StringComparer.CurrentCultureIgnoreCase)
            .Take(limit)
            .ToList();

        var entryIds = rankedEntries.Select(e => e.Id).ToList();
        var myWords = await db.Words.AsNoTracking()
            .Where(w => w.DictEntryId != null && entryIds.Contains(w.DictEntryId.Value))
            .Select(w => new { w.Id, EntryId = w.DictEntryId!.Value })
            .ToListAsync(ct);
        var myWordByEntry = myWords
            .GroupBy(x => x.EntryId)
            .ToDictionary(g => g.Key, g => g.First().Id);

        return new SearchResult(
            query,
            direction,
            rankedWords,
            rankedEntries.Select(e => DictEntryDto.From(e, myWordByEntry.TryGetValue(e.Id, out var id) ? id : null)).ToList());
    }

    /// <summary>FTS5 expression: all query words as prefixes in the target column, e.g. <c>de : ("zu"* "haus"*)</c>.</summary>
    internal static string? BuildMatch(string normalized, string column)
    {
        var tokens = TokenRegex().Matches(normalized).Select(m => m.Value).ToList();
        if (tokens.Count == 0)
            return null;
        return $"{column} : ({string.Join(' ', tokens.Select(t => $"\"{t}\"*"))})";
    }

    private Task<List<int>> MatchWordIdsAsync(string match, CancellationToken ct) =>
        db.Database
            .SqlQuery<int>($"SELECT rowid AS Value FROM words_fts WHERE words_fts MATCH {match} ORDER BY rank LIMIT {CandidateLimit}")
            .ToListAsync(ct);

    private Task<List<int>> MatchDictIdsAsync(string match, CancellationToken ct) =>
        db.Database
            .SqlQuery<int>($"SELECT rowid AS Value FROM dict_fts WHERE dict_fts MATCH {match} ORDER BY rank LIMIT {CandidateLimit}")
            .ToListAsync(ct);

    private static int RankDe(string query, params string?[] forms)
    {
        var variants = forms
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .SelectMany(f => TextNormalizer.DeVariants(f!))
            .ToList();
        if (variants.Contains(query)) return 0;
        if (variants.Any(v => v.StartsWith(query, StringComparison.Ordinal))) return 1;
        return 2;
    }

    private static int RankRu(string ru, string query)
    {
        var translations = ru
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(TextNormalizer.NormalizeRu)
            .ToList();
        if (translations.Contains(query)) return 0;
        if (translations.Any(t => t.StartsWith(query, StringComparison.Ordinal))) return 1;
        return 2;
    }

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex TokenRegex();
}
