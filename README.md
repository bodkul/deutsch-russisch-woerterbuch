# deutsch-russisch-woerterbuch

**English** | [Deutsch](./README.de.md) | [Русский](./README.ru.md)

A personal German–Russian dictionary: DE↔RU search over the open FreeDict dictionary (26,809 words) plus your own word list with article, plural and tags. Runs locally. The interface is available in English, German and Russian (switcher in the header).

**Stack:** ASP.NET Core 10 (Minimal API) · EF Core + SQLite (FTS5) · .NET Aspire 13 · React + Vite + TypeScript · Tailwind + shadcn/ui · TanStack Query.

## Getting started

Requires the .NET 10 SDK and Node.js 24.

```bash
dotnet tool restore                                  # dotnet-ef
dotnet run --project backend/Woerterbuch.Importer    # once: download and import the dictionary
dotnet run --project backend/Woerterbuch.AppHost     # API + frontend + Aspire dashboard
```

The dashboard link is printed to the console. Open the frontend from the dashboard (the `frontend` resource). The import can also be started from the dashboard with the ▶ button on the `importer` resource.

Without Aspire: `dotnet run --project backend/Woerterbuch.Api`, and in another terminal `cd frontend && npm run dev`.

## Structure

```
backend/
  Woerterbuch.AppHost/         Aspire: orchestrates api, frontend, importer
  Woerterbuch.ServiceDefaults/ OpenTelemetry, health checks
  Woerterbuch.Core/            entities, DbContext, migrations, normalization, search
  Woerterbuch.Api/             /api/search, /api/words, /api/tags, /api/dict/{id}
  Woerterbuch.Importer/        FreeDict TEI import (.tar.xz)
  Woerterbuch.Tests/           xUnit
frontend/                      React app (src/pages, src/components, src/api, src/i18n)
data/                          woerterbuch.db database and the downloaded dictionary (not in git)
```

## Search

- The query language is detected automatically: Cyrillic means searching the translations.
- Case does not matter, and umlauts can be typed any way: `Häuser` = `haeuser` = `hauser`, `Straße` = `strasse`, `ёж` = `еж`.
- Search matches word prefixes (FTS5). Words from your own list come first, then the dictionary. Exact matches rank higher.
- Words in your own list are also found by their plural form.

## Keeping your words safe

Words are stored in `data/woerterbuch.db`. This folder is not tracked by git.

- **Auto-backup:** on every API start (before and after migrations), before a dictionary import and before a word import, your words are saved to `data/backups/meine-woerter-*.json`. The last 30 copies are kept. An empty list and an unchanged list are not saved, so a failure cannot push out good copies.
- **Export / import:** the **Export** / **Import** buttons on the **My words** page. Keep the exported file off your computer (OneDrive, a USB stick). Import only adds words that are not there yet (word and article are compared) and deletes nothing.
- **Restoring from an auto-backup:** **Import** → pick a file from `data/backups/`.
- **Deleting a word** can be undone from the notification (10 seconds).

## Development

```bash
dotnet test                                                       # backend tests
dotnet ef migrations add <Name> --project backend/Woerterbuch.Core --startup-project backend/Woerterbuch.Api -o Migrations
cd frontend && npm run gen:api                                    # TS types from OpenAPI (the API must be running)
cd frontend && npm run lint && npm run build
```

Migrations are applied automatically when the API starts.

## Dictionary license

[FreeDict](https://freedict.org) deu-rus, data from [WikDict](https://www.wikdict.com) / Wiktionary, licensed under [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/). The dictionary itself is not part of the repository: the importer downloads it.
