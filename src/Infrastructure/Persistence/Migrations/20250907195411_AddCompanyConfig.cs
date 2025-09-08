using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_config",
                columns: table => new
                {
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CxcAccountId = table.Column<long>(type: "bigint", nullable: true),
                    CxpAccountId = table.Column<long>(type: "bigint", nullable: true),
                    IvaVentasAccountId = table.Column<long>(type: "bigint", nullable: true),
                    IvaComprasAccountId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_config", x => x.CompanyId);
                });

            migrationBuilder.UpdateData(
                table: "company",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 7, 19, 54, 10, 287, DateTimeKind.Utc).AddTicks(9629), new DateTime(2025, 9, 7, 19, 54, 10, 287, DateTimeKind.Utc).AddTicks(9630) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_config");

            migrationBuilder.UpdateData(
                table: "company",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 7, 6, 51, 29, 506, DateTimeKind.Utc).AddTicks(750), new DateTime(2025, 9, 7, 6, 51, 29, 506, DateTimeKind.Utc).AddTicks(750) });
        }
    }
}
