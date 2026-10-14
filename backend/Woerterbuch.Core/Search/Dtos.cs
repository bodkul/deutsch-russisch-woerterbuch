using Woerterbuch.Core.Entities;

namespace Woerterbuch.Core.Search;

public record WordDto(
    int Id,
    string De,
    string? Article,
    string? Plural,
    string? Pos,
    string Ru,
    IReadOnlyList<string> Tags,
    int? DictEntryId,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static WordDto From(Word w) => new(
        w.Id, w.De, w.Article, w.Plural, w.Pos, w.Ru,
        w.Tags, w.DictEntryId, w.CreatedAt, w.UpdatedAt);
}

public record DictSenseDto(IReadOnlyList<string> Translations, string? Definition);

/// <param name="MyWordId">Id of the word in the personal list, if the entry has already been added.</param>
public record DictEntryDto(
    int Id,
    string De,
    string? Gender,
    string? Pos,
    string? Pron,
    IReadOnlyList<DictSenseDto> Senses,
    int? MyWordId)
{
    public static DictEntryDto From(DictEntry e, int? myWordId) => new(
        e.Id, e.De, e.Gender, e.Pos, e.Pron,
        e.Senses.Select(s => new DictSenseDto(s.Translations, s.Definition)).ToList(),
        myWordId);
}

/// <param name="Direction">"de" for a German query, "ru" for a Russian one.</param>
public record SearchResult(
    string Query,
    string Direction,
    IReadOnlyList<WordDto> Words,
    IReadOnlyList<DictEntryDto> Dict);
