# deutsch-russisch-woerterbuch

[English](./README.md) | [Deutsch](./README.de.md) | **Русский**

Личный немецко-русский словарь: поиск DE↔RU по открытому словарю FreeDict (26 809 слов) и свой список слов с артиклем, мн. числом и тегами. Работает локально. Интерфейс на английском, немецком и русском (переключатель в шапке).

**Стек:** ASP.NET Core 10 (Minimal API) · EF Core + SQLite (FTS5) · .NET Aspire 13 · React + Vite + TypeScript · Tailwind + shadcn/ui · TanStack Query.

## Запуск

Нужны .NET 10 SDK и Node.js 24.

```bash
dotnet tool restore                                  # dotnet-ef
dotnet run --project backend/Woerterbuch.Importer    # один раз: скачать и импортировать словарь
dotnet run --project backend/Woerterbuch.AppHost     # API + фронтенд + дашборд Aspire
```

Ссылка на дашборд печатается в консоли. Фронтенд открывается из дашборда (ресурс `frontend`). Импорт можно запустить и из дашборда кнопкой ▶ у ресурса `importer`.

Без Aspire: `dotnet run --project backend/Woerterbuch.Api`, а в другом терминале `cd frontend && npm run dev`.

## Структура

```
backend/
  Woerterbuch.AppHost/         Aspire: оркестрация api, frontend, importer
  Woerterbuch.ServiceDefaults/ OpenTelemetry, health checks
  Woerterbuch.Core/            сущности, DbContext, миграции, нормализация, поиск
  Woerterbuch.Api/             /api/search, /api/words, /api/tags, /api/dict/{id}
  Woerterbuch.Importer/        импорт FreeDict TEI (.tar.xz)
  Woerterbuch.Tests/           xUnit
frontend/                      React-приложение (src/pages, src/components, src/api, src/i18n)
data/                          база woerterbuch.db и скачанный словарь (не в git)
```

## Поиск

- Язык запроса определяется сам: кириллица означает поиск по переводам.
- Регистр не важен, умлауты можно писать как угодно: `Häuser` = `haeuser` = `hauser`, `Straße` = `strasse`, `ёж` = `еж`.
- Поиск идёт по префиксам слов (FTS5). Сначала показываются слова из личного списка, затем словарь. Точные совпадения стоят выше.
- В личном списке слово находится и по форме мн. числа.

## Сохранность своих слов

Слова хранятся в `data/woerterbuch.db`. Эта папка не попадает в git.

- **Автобэкап:** при каждом запуске API (до и после миграций), перед импортом словаря и перед импортом слов они сохраняются в `data/backups/meine-woerter-*.json`. Хранятся последние 30 копий. Пустой список и неизменённый список не сохраняются, чтобы сбой не вытеснил хорошие копии.
- **Экспорт / импорт:** кнопки **Экспорт** / **Импорт** на странице **Мои слова**. Экспортированный файл стоит держать вне компьютера (OneDrive, флешка). Импорт только добавляет слова, которых ещё нет (сравниваются слово и артикль), и ничего не удаляет.
- **Восстановление из автобэкапа:** **Импорт** → выбрать файл из `data/backups/`.
- **Удаление слова** можно отменить в уведомлении (10 секунд).

## Разработка

```bash
dotnet test                                                       # тесты бэкенда
dotnet ef migrations add <Name> --project backend/Woerterbuch.Core --startup-project backend/Woerterbuch.Api -o Migrations
cd frontend && npm run gen:api                                    # TS-типы из OpenAPI (API должен быть запущен)
cd frontend && npm run lint && npm run build
```

Миграции применяются автоматически при старте API.

## Лицензия

Код распространяется по [лицензии MIT](LICENSE).

## Лицензия словаря

[FreeDict](https://freedict.org) deu-rus, данные [WikDict](https://www.wikdict.com) / Wiktionary, лицензия [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/). Сам словарь в репозиторий не входит: его скачивает импортёр.
