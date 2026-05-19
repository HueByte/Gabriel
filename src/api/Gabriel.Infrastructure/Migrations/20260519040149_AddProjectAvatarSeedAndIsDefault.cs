using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gabriel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAvatarSeedAndIsDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AvatarSeed",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Backfill: existing projects need real seeds (default 0 would make
            // every project render the same avatar) and any project that was
            // auto-created as "Default" needs its IsDefault flag flipped.
            // SQLite's random() returns a signed 64-bit int — we mask it into
            // the [1, 2^32-1] window the client expects (same range as
            // Conversation.AvatarSeed) so the seed round-trips through JSON
            // Number safely.
            migrationBuilder.Sql(
                "UPDATE \"Projects\" " +
                "SET \"AvatarSeed\" = (abs(random()) % 4294967295) + 1 " +
                "WHERE \"AvatarSeed\" = 0;");

            migrationBuilder.Sql(
                "UPDATE \"Projects\" SET \"IsDefault\" = 1 WHERE \"Name\" = 'Default';");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwnerUserId_IsDefault",
                table: "Projects",
                columns: new[] { "OwnerUserId", "IsDefault" },
                filter: "\"IsDefault\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_OwnerUserId_IsDefault",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AvatarSeed",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Projects");
        }
    }
}
