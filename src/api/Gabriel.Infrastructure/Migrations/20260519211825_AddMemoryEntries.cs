using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gabriel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMemoryEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemoryEntries_UserId_ProjectId_Name",
                table: "MemoryEntries",
                columns: new[] { "UserId", "ProjectId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemoryEntries_UserId_ProjectId_UpdatedAt",
                table: "MemoryEntries",
                columns: new[] { "UserId", "ProjectId", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemoryEntries");
        }
    }
}
