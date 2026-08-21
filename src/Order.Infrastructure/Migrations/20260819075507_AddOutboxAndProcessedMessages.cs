using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolverineApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxAndProcessedMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageType = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Error = table.Column<string>(type: "TEXT", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConsumerName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_MessageId_ConsumerName",
                table: "ProcessedMessages",
                columns: new[] { "MessageId", "ConsumerName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "ProcessedMessages");
        }
    }
}
