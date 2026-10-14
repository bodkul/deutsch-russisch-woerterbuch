using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Woerterbuch.Core.Migrations
{
    /// <summary>Removes "Examples" and "Notes" from the personal list.</summary>
    public partial class RemoveExamplesAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Native DROP COLUMN (SQLite 3.35+) instead of DropColumn: EF would rebuild the Words table,
            // and the words_fts_* FTS triggers would be lost along with the old table.
            migrationBuilder.Sql("""ALTER TABLE "Words" DROP COLUMN "Examples";""");
            migrationBuilder.Sql("""ALTER TABLE "Words" DROP COLUMN "Notes";""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Examples",
                table: "Words",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Words",
                type: "TEXT",
                nullable: true);
        }
    }
}
