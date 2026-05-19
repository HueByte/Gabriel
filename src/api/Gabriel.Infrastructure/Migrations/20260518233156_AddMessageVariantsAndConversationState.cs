using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gabriel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageVariantsAndConversationState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActiveVariant",
                table: "Messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "VariantGroupId",
                table: "Messages",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "StateJson",
                table: "Conversations",
                type: "TEXT",
                nullable: true);

            // Backfill existing rows: every legacy message is its own singleton
            // variant (group id = own id) and is the active one. Without this,
            // the new columns would land as inactive/empty-guid for all
            // historical messages — every conversation would render empty and
            // the variant index would treat them as one giant fake group.
            // No-op on a fresh DB (0 rows updated either way).
            migrationBuilder.Sql("UPDATE \"Messages\" SET \"IsActiveVariant\" = 1");
            migrationBuilder.Sql("UPDATE \"Messages\" SET \"VariantGroupId\" = \"Id\"");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_VariantGroupId",
                table: "Messages",
                columns: new[] { "ConversationId", "VariantGroupId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ConversationId_VariantGroupId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsActiveVariant",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "VariantGroupId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "StateJson",
                table: "Conversations");
        }
    }
}
