using System.Xml;
using System.Xml.Linq;
using SharpCompress.Readers;
using Woerterbuch.Core.Entities;

namespace Woerterbuch.Importer;

/// <summary>Streaming reader for FreeDict TEI: entry → form/orth, gramGrp/pos|gen, sense/cit/quote, sense/sense/def.</summary>
public static class FreeDictReader
{
    private static readonly XNamespace Tei = "http://www.tei-c.org/ns/1.0";

    /// <summary>Opens a .tei directly or finds the .tei inside a .tar.xz archive.</summary>
    public static Stream OpenTei(string path)
    {
        if (path.EndsWith(".tei", StringComparison.OrdinalIgnoreCase))
            return File.OpenRead(path);

        var file = File.OpenRead(path);
        var reader = ReaderFactory.OpenReader(file);
        while (reader.MoveToNextEntry())
        {
            if (!reader.Entry.IsDirectory && reader.Entry.Key?.EndsWith(".tei", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Copy into memory: ~14 MB, but the stream can then be read independently of the archive.
                var ms = new MemoryStream();
                reader.WriteEntryTo(ms);
                ms.Position = 0;
                reader.Dispose();
                file.Dispose();
                return ms;
            }
        }

        reader.Dispose();
        file.Dispose();
        throw new InvalidOperationException($"No .tei file found in archive {path}");
    }

    public static IEnumerable<DictEntry> ReadEntries(Stream tei)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null,
            IgnoreWhitespace = true,
        };

        using var reader = XmlReader.Create(tei, settings);
        reader.MoveToContent();
        while (!reader.EOF)
        {
            // ReadFrom itself advances the reader to the next node, so Read() is only in the else branch.
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "entry" && reader.NamespaceURI == Tei.NamespaceName)
            {
                if (XNode.ReadFrom(reader) is XElement element && Parse(element) is { } entry)
                    yield return entry;
            }
            else
            {
                reader.Read();
            }
        }
    }

    public static DictEntry? Parse(XElement entry)
    {
        var form = entry.Element(Tei + "form");
        var de = form?.Element(Tei + "orth")?.Value.Trim();
        if (string.IsNullOrEmpty(de))
            return null;

        var gram = entry.Element(Tei + "gramGrp");
        var senses = entry.Elements(Tei + "sense")
            .Select(s => new DictSense
            {
                Translations = s.Elements(Tei + "cit")
                    .Where(c => (string?)c.Attribute("type") == "trans")
                    .Elements(Tei + "quote")
                    .Select(q => q.Value.Trim())
                    .Where(q => q.Length > 0)
                    .Distinct()
                    .ToList(),
                Definition = s.Elements(Tei + "sense").Elements(Tei + "def").FirstOrDefault()?.Value.Trim(),
            })
            .Where(s => s.Translations.Count > 0)
            .ToList();

        if (senses.Count == 0)
            return null;

        return new DictEntry
        {
            De = de,
            Pos = gram?.Element(Tei + "pos")?.Value.Trim(),
            Gender = (gram?.Element(Tei + "gen")?.Value.Trim()) switch
            {
                "masc" => "der",
                "fem" => "die",
                "neut" => "das",
                _ => null,
            },
            Pron = form!.Element(Tei + "pron")?.Value.Trim(),
            Ru = string.Join("; ", senses.Select(s => string.Join(", ", s.Translations))),
            Senses = senses,
        };
    }
}
