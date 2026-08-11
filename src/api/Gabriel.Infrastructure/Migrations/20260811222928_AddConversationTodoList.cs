using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gabriel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationTodoList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TodoListJson",
                table: "Conversations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TodoListJson",
                table: "Conversations");
        }
    }
}
