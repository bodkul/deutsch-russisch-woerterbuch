using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Woerterbuch.Core.Migrations
{
    /// <summary>
    /// FTS5 full-text indexes on the normalized columns (DeNorm, RuNorm).
    /// rowid in the FTS table = entry Id; kept in sync by triggers.
    /// </summary>
    public partial class FullTextSearch : Migration
    {
        private static readonly (string Table, string Fts)[] Tables =
        [
            ("DictEntries", "dict_fts"),
            ("Words", "words_fts"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, fts) in Tables)
            {
                migrationBuilder.Sql($"CREATE VIRTUAL TABLE {fts} USING fts5(de, ru, tokenize = 'unicode61 remove_diacritics 0');");
                migrationBuilder.Sql($"INSERT INTO {fts}(rowid, de, ru) SELECT Id, DeNorm, RuNorm FROM {table};");
                migrationBuilder.Sql($"""
                    CREATE TRIGGER {fts}_ai AFTER INSERT ON {table} BEGIN
                        INSERT INTO {fts}(rowid, de, ru) VALUES (new.Id, new.DeNorm, new.RuNorm);
                    END;
                    """);
                migrationBuilder.Sql($"""
                    CREATE TRIGGER {fts}_ad AFTER DELETE ON {table} BEGIN
                        DELETE FROM {fts} WHERE rowid = old.Id;
                    END;
                    """);
                migrationBuilder.Sql($"""
                    CREATE TRIGGER {fts}_au AFTER UPDATE ON {table} BEGIN
                        DELETE FROM {fts} WHERE rowid = old.Id;
                        INSERT INTO {fts}(rowid, de, ru) VALUES (new.Id, new.DeNorm, new.RuNorm);
                    END;
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (_, fts) in Tables)
            {
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS {fts}_ai;");
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS {fts}_ad;");
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS {fts}_au;");
                migrationBuilder.Sql($"DROP TABLE IF EXISTS {fts};");
            }
        }
    }
}
