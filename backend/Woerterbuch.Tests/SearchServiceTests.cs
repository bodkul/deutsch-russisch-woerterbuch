using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Woerterbuch.Core;
using Woerterbuch.Core.Entities;
using Woerterbuch.Core.Search;

namespace Woerterbuch.Tests;

/// <summary>Search against real SQLite (in-memory) with migrations, so FTS5 and triggers are tested too.</summary>
public sealed class SearchServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private WoerterbuchDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _db = new WoerterbuchDbContext(new DbContextOptionsBuilder<WoerterbuchDbContext>().UseSqlite(_connection).Options);
        await _db.Database.MigrateAsync();

        _db.DictEntries.AddRange(
            Entry("Haus", "das", "дом"),
            Entry("Hausaufgabe", "die", "домашнее задание"),
            Entry("Krankenhaus", "das", "больница"),
            Entry("Straße", "die", "улица", "дорога"),
            Entry("Igel", "der", "ёж"),
            Entry("schön", null, "красивый"));
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private SearchService Service => new(_db);

    [Fact]
    public async Task ExactMatchComesFirst_AndPrefixMatchesFollow()
    {
        var result = await Service.SearchAsync("haus");

        Assert.Equal("de", result.Direction);
        Assert.Equal(["Haus", "Hausaufgabe"], result.Dict.Select(e => e.De));
    }

    [Theory]
    [InlineData("strasse")]
    [InlineData("Straße")]
    [InlineData("STRASSE")]
    public async Task SharpS_IsFolded(string query) =>
        Assert.Equal("Straße", Assert.Single((await Service.SearchAsync(query)).Dict).De);

    [Theory]
    [InlineData("schön")]
    [InlineData("schoen")]
    [InlineData("schon")]
    public async Task Umlauts_MatchAllSpellings(string query) =>
        Assert.Equal("schön", Assert.Single((await Service.SearchAsync(query)).Dict).De);

    [Theory]
    [InlineData("еж")]
    [InlineData("ёж")]
    public async Task RussianQuery_SearchesTranslations(string query)
    {
        var result = await Service.SearchAsync(query);

        Assert.Equal("ru", result.Direction);
        Assert.Equal("Igel", Assert.Single(result.Dict).De);
    }

    [Fact]
    public async Task PersonalWords_FoundByPlural_AndLinkedEntryIsMarked()
    {
        var haus = await _db.DictEntries.SingleAsync(e => e.De == "Haus");
        var word = new Word { De = "Haus", Article = "das", Plural = "Häuser", Ru = "дом", DictEntryId = haus.Id };
        _db.Words.Add(word);
        await _db.SaveChangesAsync();

        var byPlural = await Service.SearchAsync("hauser");
        Assert.Equal("Haus", Assert.Single(byPlural.Words).De);

        var bySingular = await Service.SearchAsync("Haus");
        Assert.Equal(word.Id, bySingular.Dict.First().MyWordId);
    }

    [Fact]
    public async Task UpdatedWord_IsReindexed()
    {
        var word = new Word { De = "Tisch", Ru = "стол" };
        _db.Words.Add(word);
        await _db.SaveChangesAsync();

        word.Ru = "письменный стол";
        await _db.SaveChangesAsync();
        Assert.Single((await Service.SearchAsync("письменный")).Words);

        _db.Words.Remove(word);
        await _db.SaveChangesAsync();
        Assert.Empty((await Service.SearchAsync("стол")).Words);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\"*()")]
    public async Task EmptyOrPunctuationQuery_ReturnsNothing(string query)
    {
        var result = await Service.SearchAsync(query);
        Assert.Empty(result.Dict);
        Assert.Empty(result.Words);
    }

    private static DictEntry Entry(string de, string? gender, params string[] ru) => new()
    {
        De = de,
        Gender = gender,
        Pos = gender is null ? "adj" : "n",
        Ru = string.Join(", ", ru),
        Senses = [new DictSense { Translations = [.. ru] }],
    };
}
