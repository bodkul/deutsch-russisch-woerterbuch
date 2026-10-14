using System.Text;
using Woerterbuch.Importer;

namespace Woerterbuch.Tests;

public class FreeDictReaderTests
{
    private const string Tei = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE TEI SYSTEM "freedict-P5.dtd">
        <TEI xmlns="http://www.tei-c.org/ns/1.0">
          <text><body>
            <entry>
              <form><orth>Haus</orth><pron>haʊ̯s</pron></form>
              <gramGrp><pos>n</pos><gen>neut</gen></gramGrp>
              <sense>
                <cit type="trans" xml:lang="ru"><quote>дом</quote></cit>
                <sense><def>Gebäude</def></sense>
                <sense><def>zweite Definition</def></sense>
              </sense>
            </entry>
            <entry>
              <form><orth>gehen</orth></form>
              <gramGrp><pos>v</pos></gramGrp>
              <sense><cit type="trans" xml:lang="ru"><quote>идти</quote><quote>ходить</quote></cit></sense>
              <sense><cit type="trans" xml:lang="ru"><quote>уходить</quote></cit></sense>
            </entry>
            <entry>
              <form><orth>ohneÜbersetzung</orth></form>
              <sense><sense><def>nur Definition</def></sense></sense>
            </entry>
          </body></text>
        </TEI>
        """;

    [Fact]
    public void ReadEntries_ParsesAllEntriesWithTranslations()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Tei));

        var entries = FreeDictReader.ReadEntries(stream).ToList();

        // Consecutive <entry> elements must not be lost; an entry without translations is skipped.
        Assert.Equal(["Haus", "gehen"], entries.Select(e => e.De));

        var haus = entries[0];
        Assert.Equal("das", haus.Gender);
        Assert.Equal("n", haus.Pos);
        Assert.Equal("haʊ̯s", haus.Pron);
        Assert.Equal("Gebäude", Assert.Single(haus.Senses).Definition);

        var gehen = entries[1];
        Assert.Null(gehen.Gender);
        Assert.Equal("идти, ходить; уходить", gehen.Ru);
        Assert.Equal(2, gehen.Senses.Count);
    }
}
