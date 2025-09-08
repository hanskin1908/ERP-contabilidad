using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalLineCategoryAndThirdValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "journal_line",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "company",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 7, 6, 51, 29, 506, DateTimeKind.Utc).AddTicks(750), new DateTime(2025, 9, 7, 6, 51, 29, 506, DateTimeKind.Utc).AddTicks(750) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "journal_line");

            migrationBuilder.UpdateData(
                table: "company",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 7, 5, 45, 5, 439, DateTimeKind.Utc).AddTicks(3515), new DateTime(2025, 9, 7, 5, 45, 5, 439, DateTimeKind.Utc).AddTicks(3515) });
        }
    }
}
