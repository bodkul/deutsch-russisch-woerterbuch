using System.ComponentModel.DataAnnotations;

namespace Woerterbuch.Api.Models;

/// <summary>Word form data (create and edit).</summary>
public record WordInput(
    [property: Required, StringLength(200)] string De,
    [property: AllowedValues("der", "die", "das", null)] string? Article,
    [property: StringLength(200)] string? Plural,
    [property: StringLength(50)] string? Pos,
    [property: Required, StringLength(2000)] string Ru,
    List<string>? Tags,
    int? DictEntryId);
