using System.Globalization;
using System.Text;

namespace Woerterbuch.Core;

/// <summary>
/// Text normalization for search: case, umlauts, ß, ё.
/// </summary>
/// <remarks>
/// German text is indexed in two variants: the "ae" form (Häuser → haeuser) and the base form
/// (Häuser → hauser), while the query is converted to the "ae" form. So "Häuser", "haeuser" and "hauser" all match.
/// </remarks>
public static class TextNormalizer
{
    /// <summary>German text → "ae" form: häuser → haeuser, straße → strasse.</summary>
    public static string NormalizeDe(string text) => Fold(text, umlautsAsE: true);

    /// <summary>Text for the index: the "ae" form and, if different, the base form.</summary>
    public static string DeIndexText(string text) => string.Join(' ', DeVariants(text));

    /// <summary>Spelling variants: the "ae" form and the base form (no duplicates).</summary>
    public static IReadOnlyList<string> DeVariants(string text)
    {
        var withE = Fold(text, umlautsAsE: true);
        var plain = Fold(text, umlautsAsE: false);
        return withE == plain ? [withE] : [withE, plain];
    }

    /// <summary>Russian text: lowercase, ё → е.</summary>
    public static string NormalizeRu(string text) =>
        text.ToLowerInvariant().Replace('ё', 'е').Trim();

    public static bool ContainsCyrillic(string text) =>
        text.Any(c => c is >= 'Ѐ' and <= 'ӿ');

    private static string Fold(string text, bool umlautsAsE)
    {
        var sb = new StringBuilder(text.Length + 4);
        foreach (var c in text.ToLowerInvariant())
        {
            switch (c)
            {
                case 'ä': sb.Append(umlautsAsE ? "ae" : "a"); break;
                case 'ö': sb.Append(umlautsAsE ? "oe" : "o"); break;
                case 'ü': sb.Append(umlautsAsE ? "ue" : "u"); break;
                case 'ß': sb.Append("ss"); break;
                default: sb.Append(c); break;
            }
        }

        // Other diacritics (é, à, …) are simply stripped.
        var decomposed = sb.ToString().Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                result.Append(c);
        }
        return result.ToString().Normalize(NormalizationForm.FormC).Trim();
    }
}
