using System.IO.Compression;
using System.Text.Json;
using Woerterbuch.Core.Entities;

namespace Woerterbuch.Importer;

/// <summary>Reader for the Russian Wiktionary dump from kaikki.org (.jsonl or .jsonl.gz): German headwords with Russian glosses.</summary>
public static class WiktionaryReader
{
    private static readonly Dictionary<string, string> PosCodes = new()
    {
        ["noun"] = "n",
        ["verb"] = "v",
        ["adj"] = "adj",
        ["adv"] = "adv",
    };

    public static Stream Open(string path)
    {
        var file = File.OpenRead(path);
        return path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
            ? new GZipStream(file, CompressionMode.Decompress)
            : file;
    }

    public static IEnumerable<DictEntry> ReadEntries(Stream jsonl)
    {
        using var reader = new StreamReader(jsonl);
        while (reader.ReadLine() is { } line)
        {
            // Cheap pre-filter: German headwords have lang_code right after pos, the dump has millions of lines.
            if (!line.Contains("\"lang_code\": \"de\"", StringComparison.Ordinal))
                continue;

            using var doc = JsonDocument.Parse(line);
            if (Parse(doc.RootElement) is { } entry)
                yield return entry;
        }
    }

    public static DictEntry? Parse(JsonElement root)
    {
        if (GetString(root, "lang_code") != "de")
            return null;

        var de = GetString(root, "word")?.Trim();
        if (string.IsNullOrEmpty(de) || !root.TryGetProperty("senses", out var sensesEl))
            return null;

        var senses = new List<DictSense>();
        foreach (var sense in sensesEl.EnumerateArray())
        {
            if (!sense.TryGetProperty("glosses", out var glosses))
                continue;

            var text = string.Join("; ", glosses.EnumerateArray()
                .Select(g => g.GetString()?.Trim())
                .Where(g => !string.IsNullOrEmpty(g)));
            if (text.Length > 0)
                senses.Add(new DictSense { Translations = [text] });
        }

        if (senses.Count == 0)
            return null;

        var pos = GetString(root, "pos");
        return new DictEntry
        {
            De = de,
            Pos = pos is null ? null : PosCodes.GetValueOrDefault(pos, pos),
            Gender = GetGender(root),
            Pron = GetPron(root),
            Forms = GetPluralForms(root, de),
            Ru = string.Join("; ", senses.Select(s => s.Translations[0])),
            Senses = senses,
        };
    }

    private static string? GetPluralForms(JsonElement root, string de)
    {
        if (!root.TryGetProperty("forms", out var forms))
            return null;

        var result = new List<string>();
        foreach (var f in forms.EnumerateArray())
        {
            if (!f.TryGetProperty("tags", out var tags))
                continue;

            var tagList = tags.EnumerateArray().Select(t => t.GetString()).ToList();
            if (tagList.Contains("plural") && tagList.Contains("nominative")
                && GetString(f, "form")?.Trim() is { Length: > 0 } form
                && form != de && !result.Contains(form))
                result.Add(form);
        }

        return result.Count == 0 ? null : string.Join(' ', result);
    }

    private static string? GetGender(JsonElement root)
    {
        if (!root.TryGetProperty("categories", out var categories))
            return null;

        foreach (var c in categories.EnumerateArray())
        {
            switch (c.GetString())
            {
                case "Мужской род/de": return "der";
                case "Женский род/de": return "die";
                case "Средний род/de": return "das";
            }
        }

        return null;
    }

    private static string? GetPron(JsonElement root)
    {
        if (!root.TryGetProperty("sounds", out var sounds))
            return null;

        foreach (var s in sounds.EnumerateArray())
        {
            if (GetString(s, "ipa") is { Length: > 0 } ipa)
                return ipa.Trim();
        }

        return null;
    }

    private static string? GetString(JsonElement el, string name) =>
        el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}
