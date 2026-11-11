namespace Woerterbuch.Core;

/// <summary>Shared cleanup of user-provided word fields (forms and file import).</summary>
public static class WordText
{
    public static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>Trims tags, drops empty ones and duplicates (case-insensitive).</summary>
    public static List<string> CleanTags(IEnumerable<string>? tags) =>
        (tags ?? [])
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
