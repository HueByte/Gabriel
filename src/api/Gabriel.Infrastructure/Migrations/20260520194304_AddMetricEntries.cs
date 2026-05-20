using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gabriel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetricEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    System = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Metric = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetricEntries_System_CreatedAt",
                table: "MetricEntries",
                columns: new[] { "System", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricEntries_CreatedAt",
                table: "MetricEntries",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetricEntries");
        }
    }
}
