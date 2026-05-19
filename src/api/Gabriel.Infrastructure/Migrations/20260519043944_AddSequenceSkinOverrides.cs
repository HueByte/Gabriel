using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gabriel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSequenceSkinOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaletteOverride",
                table: "Projects",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatternOverride",
                table: "Projects",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaletteOverride",
                table: "Conversations",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatternOverride",
                table: "Conversations",
                type: "TEXT",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaletteOverride",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PatternOverride",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PaletteOverride",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "PatternOverride",
                table: "Conversations");
        }
    }
}
