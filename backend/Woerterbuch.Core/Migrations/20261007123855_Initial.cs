using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Woerterbuch.Core.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DictEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    De = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    Pos = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Pron = table.Column<string>(type: "TEXT", nullable: true),
                    Ru = table.Column<string>(type: "TEXT", nullable: false),
                    DeNorm = table.Column<string>(type: "TEXT", nullable: false),
                    RuNorm = table.Column<string>(type: "TEXT", nullable: false),
                    Senses = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    De = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Article = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    Plural = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Pos = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Ru = table.Column<string>(type: "TEXT", nullable: false),
                    Examples = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    DictEntryId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeNorm = table.Column<string>(type: "TEXT", nullable: false),
                    RuNorm = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Words_DictEntries_DictEntryId",
                        column: x => x.DictEntryId,
                        principalTable: "DictEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DictEntries_DeNorm",
                table: "DictEntries",
                column: "DeNorm");

            migrationBuilder.CreateIndex(
                name: "IX_Words_DictEntryId",
                table: "Words",
                column: "DictEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "DictEntries");
        }
    }
}
