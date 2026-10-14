namespace Woerterbuch.Core.Entities;

/// <summary>Entry from the imported dictionary (FreeDict deu-rus). Read-only.</summary>
public class DictEntry : INormalizable
{
    public int Id { get; set; }

    /// <summary>German headword.</summary>
    public required string De { get; set; }

    /// <summary>der / die / das or null.</summary>
    public string? Gender { get; set; }

    /// <summary>Part of speech as in FreeDict: n, v, adj, adv, …</summary>
    public string? Pos { get; set; }

    /// <summary>Transcription (IPA), the first one in the list.</summary>
    public string? Pron { get; set; }

    /// <summary>All translations in one string: senses separated by "; ", synonyms by ", ".</summary>
    public required string Ru { get; set; }

    public List<DictSense> Senses { get; set; } = [];

    public string DeNorm { get; set; } = "";
    public string RuNorm { get; set; } = "";

    public void Normalize()
    {
        DeNorm = TextNormalizer.DeIndexText(De);
        RuNorm = TextNormalizer.NormalizeRu(Ru);
    }
}

/// <summary>One sense of a word: Russian translations + German definition.</summary>
public class DictSense
{
    public List<string> Translations { get; set; } = [];
    public string? Definition { get; set; }
}
