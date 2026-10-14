# deutsch-russisch-woerterbuch

[English](./README.md) | **Deutsch** | [Русский](./README.ru.md)

Ein persönliches Deutsch-Russisch-Wörterbuch: DE↔RU-Suche im offenen Wörterbuch FreeDict (26.809 Wörter) und eine eigene Wortliste mit Artikel, Plural und Tags. Läuft lokal. Die Oberfläche gibt es auf Englisch, Deutsch und Russisch (Umschalter in der Kopfzeile).

**Stack:** ASP.NET Core 10 (Minimal API) · EF Core + SQLite (FTS5) · .NET Aspire 13 · React + Vite + TypeScript · Tailwind + shadcn/ui · TanStack Query.

## Starten

Benötigt werden das .NET 10 SDK und Node.js 24.

```bash
dotnet tool restore                                  # dotnet-ef
dotnet run --project backend/Woerterbuch.Importer    # einmalig: Wörterbuch herunterladen und importieren
dotnet run --project backend/Woerterbuch.AppHost     # API + Frontend + Aspire-Dashboard
```

Der Link zum Dashboard wird in der Konsole ausgegeben. Das Frontend öffnet man über das Dashboard (Ressource `frontend`). Der Import lässt sich auch im Dashboard über die Schaltfläche ▶ der Ressource `importer` starten.

Ohne Aspire: `dotnet run --project backend/Woerterbuch.Api` und in einem zweiten Terminal `cd frontend && npm run dev`.

## Struktur

```
backend/
  Woerterbuch.AppHost/         Aspire: orchestriert api, frontend, importer
  Woerterbuch.ServiceDefaults/ OpenTelemetry, Health Checks
  Woerterbuch.Core/            Entitäten, DbContext, Migrationen, Normalisierung, Suche
  Woerterbuch.Api/             /api/search, /api/words, /api/tags, /api/dict/{id}
  Woerterbuch.Importer/        Import von FreeDict TEI (.tar.xz)
  Woerterbuch.Tests/           xUnit
frontend/                      React-App (src/pages, src/components, src/api, src/i18n)
data/                          Datenbank woerterbuch.db und heruntergeladenes Wörterbuch (nicht in git)
```

## Suche

- Die Sprache der Anfrage wird automatisch erkannt: Kyrillisch bedeutet Suche in den Übersetzungen.
- Groß-/Kleinschreibung spielt keine Rolle, Umlaute können beliebig geschrieben werden: `Häuser` = `haeuser` = `hauser`, `Straße` = `strasse`, `ёж` = `еж`.
- Gesucht wird nach Wortanfängen (FTS5). Zuerst erscheinen Wörter aus der eigenen Liste, dann das Wörterbuch. Exakte Treffer stehen weiter oben.
- Wörter der eigenen Liste werden auch über die Pluralform gefunden.

## Sicherung der eigenen Wörter

Die Wörter liegen in `data/woerterbuch.db`. Dieser Ordner wird nicht von git erfasst.

- **Automatische Sicherung:** bei jedem Start der API (vor und nach den Migrationen), vor dem Wörterbuch-Import und vor dem Wort-Import werden die Wörter in `data/backups/meine-woerter-*.json` gespeichert. Die letzten 30 Kopien bleiben erhalten. Eine leere und eine unveränderte Liste werden nicht gespeichert, damit ein Fehler keine guten Kopien verdrängt.
- **Export / Import:** die Schaltflächen **Export** / **Import** auf der Seite **Meine Wörter**. Die exportierte Datei sollte man außerhalb des Rechners aufbewahren (OneDrive, USB-Stick). Der Import fügt nur Wörter hinzu, die noch fehlen (verglichen werden Wort und Artikel), und löscht nichts.
- **Wiederherstellen aus einer automatischen Sicherung:** **Import** → eine Datei aus `data/backups/` wählen.
- **Das Löschen eines Wortes** lässt sich in der Benachrichtigung rückgängig machen (10 Sekunden).

## Entwicklung

```bash
dotnet test                                                       # Backend-Tests
dotnet ef migrations add <Name> --project backend/Woerterbuch.Core --startup-project backend/Woerterbuch.Api -o Migrations
cd frontend && npm run gen:api                                    # TS-Typen aus OpenAPI (die API muss laufen)
cd frontend && npm run lint && npm run build
```

Migrationen werden beim Start der API automatisch angewendet.

## Lizenz

Der Code steht unter der [MIT-Lizenz](LICENSE).

## Lizenz des Wörterbuchs

[FreeDict](https://freedict.org) deu-rus, Daten von [WikDict](https://www.wikdict.com) / Wiktionary, Lizenz [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/). Das Wörterbuch selbst ist nicht Teil des Repositorys: Der Importer lädt es herunter.
