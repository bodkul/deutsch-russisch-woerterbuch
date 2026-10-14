using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Woerterbuch.Core;
using Woerterbuch.Core.Search;

namespace Woerterbuch.Api;

public static class SearchEndpoints
{
    public static RouteGroupBuilder MapSearchEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/search", async (string? q, int? limit, SearchService search, CancellationToken ct) =>
            await search.SearchAsync(q ?? "", Math.Clamp(limit ?? 30, 1, 100), ct))
            .WithName("Search")
            .WithSummary("DE↔RU search: personal words first, then the dictionary");

        api.MapGet("/dict/{id:int}", async Task<Results<Ok<DictEntryDto>, NotFound>> (int id, WoerterbuchDbContext db, CancellationToken ct) =>
        {
            var entry = await db.DictEntries.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entry is null)
                return TypedResults.NotFound();

            var myWordId = await db.Words
                .Where(w => w.DictEntryId == id)
                .Select(w => (int?)w.Id)
                .FirstOrDefaultAsync(ct);
            return TypedResults.Ok(DictEntryDto.From(entry, myWordId));
        })
        .WithName("GetDictEntry");

        return api;
    }
}
