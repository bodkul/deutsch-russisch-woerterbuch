using Woerterbuch.Core;

namespace Woerterbuch.Tests;

public class TextNormalizerTests
{
    [Theory]
    [InlineData("Häuser", "haeuser")]
    [InlineData("Straße", "strasse")]
    [InlineData("SCHÖN", "schoen")]
    [InlineData("Café", "cafe")]
    [InlineData("  Über  ", "ueber")]
    public void NormalizeDe_FoldsUmlautsToE(string input, string expected) =>
        Assert.Equal(expected, TextNormalizer.NormalizeDe(input));

    [Fact]
    public void DeVariants_ContainsBothSpellings() =>
        Assert.Equal(["haeuser", "hauser"], TextNormalizer.DeVariants("Häuser"));

    [Fact]
    public void DeVariants_NoDuplicateWithoutUmlauts() =>
        Assert.Equal(["haus"], TextNormalizer.DeVariants("Haus"));

    [Theory]
    [InlineData("Ёж", "еж")]
    [InlineData("ДОМ", "дом")]
    public void NormalizeRu_LowercasesAndFoldsYo(string input, string expected) =>
        Assert.Equal(expected, TextNormalizer.NormalizeRu(input));

    [Theory]
    [InlineData("дом", true)]
    [InlineData("Haus", false)]
    [InlineData("Haus дом", true)]
    public void ContainsCyrillic(string input, bool expected) =>
        Assert.Equal(expected, TextNormalizer.ContainsCyrillic(input));
}
