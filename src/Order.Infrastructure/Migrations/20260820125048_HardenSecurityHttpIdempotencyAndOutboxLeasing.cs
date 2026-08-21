using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolverineApp.Migrations
{
    /// <inheritdoc />
    public partial class HardenSecurityHttpIdempotencyAndOutboxLeasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<string>(
                name: "LockOwner",
                table: "OutboxMessages",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntilUtc",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HttpIdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Method = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RequestHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    ResponseContentType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ResponseBody = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpIdempotencyRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsAutoDiscovered = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DatasetCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SelectedFieldsJson = table.Column<string>(type: "TEXT", nullable: false),
                    FilterConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    TemplateContent = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SemanticDatasets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FieldsMetadataJson = table.Column<string>(type: "TEXT", nullable: false),
                    BaseQuerySql = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemanticDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermissionCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptAtUtc_LockedUntilUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "NextAttemptAtUtc", "LockedUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_CreatedAt",
                table: "Orders",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_OrderNumber",
                table: "Orders",
                columns: new[] { "TenantId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_Status_CreatedAt",
                table: "Orders",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_HttpIdempotencyRecords_ExpiresAtUtc",
                table: "HttpIdempotencyRecords",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_HttpIdempotencyRecords_TenantId_UserId_Method_Path_IdempotencyKey",
                table: "HttpIdempotencyRecords",
                columns: new[] { "TenantId", "UserId", "Method", "Path", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportConfigurations_TenantId_Code",
                table: "ReportConfigurations",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_TenantId_Code",
                table: "ReportTemplates",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionCode",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SemanticDatasets_Code",
                table: "SemanticDatasets",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_TenantId_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "TenantId", "UserId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HttpIdempotencyRecords");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "ReportConfigurations");

            migrationBuilder.DropTable(
                name: "ReportTemplates");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SemanticDatasets");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptAtUtc_LockedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_OrderNumber",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_Status_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TenantId_Timestamp",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "LockOwner",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LockedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");
        }
    }
}
