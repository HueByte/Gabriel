using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gabriel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Goal = table.Column<string>(type: "TEXT", nullable: false),
                    Objective = table.Column<string>(type: "TEXT", nullable: false),
                    DoneCriteria = table.Column<string>(type: "TEXT", nullable: false),
                    Constraints = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusReason = table.Column<string>(type: "TEXT", nullable: true),
                    StepsCompleted = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxSteps = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Deadline = table.Column<long>(type: "INTEGER", nullable: true),
                    ConsecutiveNoProgress = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsecutiveErrors = table.Column<int>(type: "INTEGER", nullable: false),
                    PendingGuidance = table.Column<string>(type: "TEXT", nullable: true),
                    CheckpointStep = table.Column<int>(type: "INTEGER", nullable: true),
                    CheckpointStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    CheckpointProgress = table.Column<string>(type: "TEXT", nullable: true),
                    CheckpointQuestion = table.Column<string>(type: "TEXT", nullable: true),
                    CheckpointSummary = table.Column<string>(type: "TEXT", nullable: true),
                    FinalSummary = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    FinishedAt = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentTasks_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentTaskLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Step = table.Column<int>(type: "INTEGER", nullable: true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentTaskLogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentTaskLogEntries_AgentTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "AgentTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentTaskLogEntries_TaskId_CreatedAt",
                table: "AgentTaskLogEntries",
                columns: new[] { "TaskId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentTasks_ConversationId",
                table: "AgentTasks",
                column: "ConversationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentTasks_Status_UpdatedAt",
                table: "AgentTasks",
                columns: new[] { "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentTasks_UserId_ProjectId_UpdatedAt",
                table: "AgentTasks",
                columns: new[] { "UserId", "ProjectId", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentTaskLogEntries");

            migrationBuilder.DropTable(
                name: "AgentTasks");
        }
    }
}
