namespace Woerterbuch.Core.Entities;

/// <summary>Word from the personal list.</summary>
public class Word : INormalizable
{
    public int Id { get; set; }

    public required string De { get; set; }

    /// <summary>der / die / das or null.</summary>
    public string? Article { get; set; }

    public string? Plural { get; set; }
    public string? Pos { get; set; }
    public required string Ru { get; set; }
    public List<string> Tags { get; set; } = [];

    /// <summary>Dictionary entry the word was added from (if added from the dictionary).</summary>
    public int? DictEntryId { get; set; }
    public DictEntry? DictEntry { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string DeNorm { get; set; } = "";
    public string RuNorm { get; set; } = "";

    public void Normalize()
    {
        // Plural is searchable too: "Häuser" finds "Haus".
        DeNorm = TextNormalizer.DeIndexText(string.IsNullOrWhiteSpace(Plural) ? De : $"{De} {Plural}");
        RuNorm = TextNormalizer.NormalizeRu(Ru);
    }
}
