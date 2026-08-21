using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolverineApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PostgreSqlProductionHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.Sql("""
                ALTER TABLE "TenantMemberships" ALTER COLUMN "IsActive" DROP DEFAULT;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsActive" DROP DEFAULT;
                ALTER TABLE "Roles" ALTER COLUMN "IsSystemRole" DROP DEFAULT;
                ALTER TABLE "Roles" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsSystemDefault" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsActive" DROP DEFAULT;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsActive" DROP DEFAULT;
                ALTER TABLE "Permissions" ALTER COLUMN "IsSystem" DROP DEFAULT;
                ALTER TABLE "Permissions" ALTER COLUMN "IsAutoDiscovered" DROP DEFAULT;
                ALTER TABLE "Orders" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "AuditLogs" ALTER COLUMN "IsSuccess" DROP DEFAULT;
                ALTER TABLE "UserRoles" ALTER COLUMN "RoleId" DROP DEFAULT;
                ALTER TABLE "UserRoles" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "TenantMemberships" ALTER COLUMN "CreatedAtUtc" DROP DEFAULT;
                ALTER TABLE "TenantMemberships" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "UpdatedAt" DROP DEFAULT;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "DeletedAt" DROP DEFAULT;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "CreatedAt" DROP DEFAULT;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "Roles" ALTER COLUMN "UpdatedAt" DROP DEFAULT;
                ALTER TABLE "Roles" ALTER COLUMN "DeletedAt" DROP DEFAULT;
                ALTER TABLE "Roles" ALTER COLUMN "CreatedAt" DROP DEFAULT;
                ALTER TABLE "Roles" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "RolePermissions" ALTER COLUMN "RoleId" DROP DEFAULT;
                ALTER TABLE "RolePermissions" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "UpdatedAt" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "DeletedAt" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "CreatedAt" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "UpdatedAt" DROP DEFAULT;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "DeletedAt" DROP DEFAULT;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "CreatedAt" DROP DEFAULT;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "ProcessedOnUtc" DROP DEFAULT;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "MessageId" DROP DEFAULT;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "Permissions" ALTER COLUMN "CreatedAt" DROP DEFAULT;
                ALTER TABLE "Permissions" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "ProcessedOnUtc" DROP DEFAULT;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "OccurredOnUtc" DROP DEFAULT;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "NextAttemptAtUtc" DROP DEFAULT;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "LockedUntilUtc" DROP DEFAULT;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "Orders" ALTER COLUMN "UpdatedAt" DROP DEFAULT;
                ALTER TABLE "Orders" ALTER COLUMN "TotalAmount" DROP DEFAULT;
                ALTER TABLE "Orders" ALTER COLUMN "DeletedAt" DROP DEFAULT;
                ALTER TABLE "Orders" ALTER COLUMN "CreatedAt" DROP DEFAULT;
                ALTER TABLE "Orders" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "UpdatedAt" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "UnitPrice" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "Total" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "OrderId" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "DeletedAt" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "CreatedAt" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "ExpiresAtUtc" DROP DEFAULT;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "CreatedAtUtc" DROP DEFAULT;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "Id" DROP DEFAULT;
                ALTER TABLE "AuditLogs" ALTER COLUMN "Timestamp" DROP DEFAULT;
                ALTER TABLE "AuditLogs" ALTER COLUMN "DurationMs" DROP DEFAULT;
                ALTER TABLE "AuditLogs" ALTER COLUMN "Id" DROP DEFAULT;
                """);

            // The previous migrations were generated against SQLite and persisted
            // UUIDs, timestamps, booleans, and decimals as TEXT/INTEGER. PostgreSQL
            // requires an explicit USING expression for these conversions.
            migrationBuilder.Sql("""
                ALTER TABLE "UserRoles" ALTER COLUMN "RoleId" TYPE uuid USING "RoleId"::uuid;
                ALTER TABLE "UserRoles" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "TenantMemberships" ALTER COLUMN "IsActive" TYPE boolean USING ("IsActive" <> 0);
                ALTER TABLE "TenantMemberships" ALTER COLUMN "CreatedAtUtc" TYPE timestamp with time zone USING "CreatedAtUtc"::timestamptz;
                ALTER TABLE "TenantMemberships" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsDeleted" TYPE boolean USING ("IsDeleted" <> 0);
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsActive" TYPE boolean USING ("IsActive" <> 0);
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "Roles" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz;
                ALTER TABLE "Roles" ALTER COLUMN "IsSystemRole" TYPE boolean USING ("IsSystemRole" <> 0);
                ALTER TABLE "Roles" ALTER COLUMN "IsDeleted" TYPE boolean USING ("IsDeleted" <> 0);
                ALTER TABLE "Roles" ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
                ALTER TABLE "Roles" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz;
                ALTER TABLE "Roles" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "RolePermissions" ALTER COLUMN "RoleId" TYPE uuid USING "RoleId"::uuid;
                ALTER TABLE "RolePermissions" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsSystemDefault" TYPE boolean USING ("IsSystemDefault" <> 0);
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsDeleted" TYPE boolean USING ("IsDeleted" <> 0);
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsActive" TYPE boolean USING ("IsActive" <> 0);
                ALTER TABLE "ReportTemplates" ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsDeleted" TYPE boolean USING ("IsDeleted" <> 0);
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsActive" TYPE boolean USING ("IsActive" <> 0);
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "ProcessedOnUtc" TYPE timestamp with time zone USING "ProcessedOnUtc"::timestamptz;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "MessageId" TYPE uuid USING "MessageId"::uuid;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "Permissions" ALTER COLUMN "IsSystem" TYPE boolean USING ("IsSystem" <> 0);
                ALTER TABLE "Permissions" ALTER COLUMN "IsAutoDiscovered" TYPE boolean USING ("IsAutoDiscovered" <> 0);
                ALTER TABLE "Permissions" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz;
                ALTER TABLE "Permissions" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "ProcessedOnUtc" TYPE timestamp with time zone USING "ProcessedOnUtc"::timestamptz;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "OccurredOnUtc" TYPE timestamp with time zone USING "OccurredOnUtc"::timestamptz;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "NextAttemptAtUtc" TYPE timestamp with time zone USING "NextAttemptAtUtc"::timestamptz;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "LockedUntilUtc" TYPE timestamp with time zone USING "LockedUntilUtc"::timestamptz;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "Orders" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz;
                ALTER TABLE "Orders" ALTER COLUMN "TotalAmount" TYPE numeric(10,2) USING "TotalAmount"::numeric(10,2);
                ALTER TABLE "Orders" ALTER COLUMN "IsDeleted" TYPE boolean USING ("IsDeleted" <> 0);
                ALTER TABLE "Orders" ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
                ALTER TABLE "Orders" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz;
                ALTER TABLE "Orders" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "OrderItems" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz;
                ALTER TABLE "OrderItems" ALTER COLUMN "UnitPrice" TYPE numeric(10,2) USING "UnitPrice"::numeric(10,2);
                ALTER TABLE "OrderItems" ALTER COLUMN "Total" TYPE numeric(10,2) USING "Total"::numeric(10,2);
                ALTER TABLE "OrderItems" ALTER COLUMN "OrderId" TYPE uuid USING "OrderId"::uuid;
                ALTER TABLE "OrderItems" ALTER COLUMN "IsDeleted" TYPE boolean USING ("IsDeleted" <> 0);
                ALTER TABLE "OrderItems" ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
                ALTER TABLE "OrderItems" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz;
                ALTER TABLE "OrderItems" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "ExpiresAtUtc" TYPE timestamp with time zone USING "ExpiresAtUtc"::timestamptz;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "CreatedAtUtc" TYPE timestamp with time zone USING "CreatedAtUtc"::timestamptz;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                ALTER TABLE "AuditLogs" ALTER COLUMN "Timestamp" TYPE timestamp with time zone USING "Timestamp"::timestamptz;
                ALTER TABLE "AuditLogs" ALTER COLUMN "IsSuccess" TYPE boolean USING ("IsSuccess" <> 0);
                ALTER TABLE "AuditLogs" ALTER COLUMN "DurationMs" TYPE bigint USING "DurationMs"::bigint;
                ALTER TABLE "AuditLogs" ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;
                """);

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptAtUtc_LockedUntilUtc",
                table: "OutboxMessages",
                newName: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptAtUtc_LockedUntilU~");

            migrationBuilder.RenameIndex(
                name: "IX_HttpIdempotencyRecords_TenantId_UserId_Method_Path_IdempotencyKey",
                table: "HttpIdempotencyRecords",
                newName: "IX_HttpIdempotencyRecords_TenantId_UserId_Method_Path_Idempote~");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserRoles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "UserRoles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "RoleId",
                table: "UserRoles",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserRoles",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TenantMemberships",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "TenantMemberships",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TenantMemberships",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "TenantMemberships",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "TenantMemberships",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "SemanticDatasets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SemanticDatasets",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "SemanticDatasets",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SemanticDatasets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SemanticDatasets",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SemanticDatasets",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "FieldsMetadataJson",
                table: "SemanticDatasets",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SemanticDatasets",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "SemanticDatasets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "SemanticDatasets",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "SemanticDatasets",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SemanticDatasets",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "SemanticDatasets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "SemanticDatasets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "BaseQuerySql",
                table: "SemanticDatasets",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SemanticDatasets",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Roles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Roles",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Roles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystemRole",
                table: "Roles",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Roles",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "Roles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Roles",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Roles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Roles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Roles",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "RolePermissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "RoleId",
                table: "RolePermissions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "PermissionCode",
                table: "RolePermissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "RolePermissions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "ReportTemplates",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "ReportTemplates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReportTemplates",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "ReportTemplates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ReportTemplates",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystemDefault",
                table: "ReportTemplates",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ReportTemplates",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ReportTemplates",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ReportTemplates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "ReportTemplates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "ReportTemplates",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ReportTemplates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReportTemplates",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ReportTemplates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ReportTemplates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ReportTemplates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ReportTemplates",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "ReportConfigurations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "ReportConfigurations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReportConfigurations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "ReportConfigurations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TemplateContent",
                table: "ReportConfigurations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "SelectedFieldsJson",
                table: "ReportConfigurations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ReportConfigurations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ReportConfigurations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ReportConfigurations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "FilterConfigJson",
                table: "ReportConfigurations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "ReportConfigurations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "ReportConfigurations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DatasetCode",
                table: "ReportConfigurations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ReportConfigurations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReportConfigurations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ReportConfigurations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ReportConfigurations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedOnUtc",
                table: "ProcessedMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "ProcessedMessages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ConsumerName",
                table: "ProcessedMessages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProcessedMessages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Resource",
                table: "Permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Module",
                table: "Permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "Permissions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAutoDiscovered",
                table: "Permissions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Permissions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "Permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Permissions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "OutboxMessages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "RetryCount",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedOnUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "OutboxMessages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OccurredOnUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MessageType",
                table: "OutboxMessages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LockedUntilUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LockOwner",
                table: "OutboxMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Error",
                table: "OutboxMessages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "OutboxMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "OutboxMessages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItems",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "OrderItems",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "OrderItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "OrderItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "OrderItems",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "HttpIdempotencyRecords",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "HttpIdempotencyRecords",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HttpIdempotencyRecords",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "ResponseStatusCode",
                table: "HttpIdempotencyRecords",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponseContentType",
                table: "HttpIdempotencyRecords",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponseBody",
                table: "HttpIdempotencyRecords",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RequestHash",
                table: "HttpIdempotencyRecords",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "HttpIdempotencyRecords",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "HttpIdempotencyRecords",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "HttpIdempotencyRecords",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "HttpIdempotencyRecords",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "HttpIdempotencyRecords",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "HttpIdempotencyRecords",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "AuditLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSuccess",
                table: "AuditLogs",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "AuditLogs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<long>(
                name: "DurationMs",
                table: "AuditLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "AuditLogs",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.Sql("""
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsDeleted" SET DEFAULT FALSE;
                ALTER TABLE "Roles" ALTER COLUMN "IsDeleted" SET DEFAULT FALSE;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsDeleted" SET DEFAULT FALSE;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsDeleted" SET DEFAULT FALSE;
                ALTER TABLE "Orders" ALTER COLUMN "IsDeleted" SET DEFAULT FALSE;
                ALTER TABLE "OrderItems" ALTER COLUMN "IsDeleted" SET DEFAULT FALSE;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.Sql("""
                ALTER TABLE "TenantMemberships" ALTER COLUMN "IsActive" DROP DEFAULT;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsActive" DROP DEFAULT;
                ALTER TABLE "Roles" ALTER COLUMN "IsSystemRole" DROP DEFAULT;
                ALTER TABLE "Roles" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsSystemDefault" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsActive" DROP DEFAULT;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsActive" DROP DEFAULT;
                ALTER TABLE "Permissions" ALTER COLUMN "IsSystem" DROP DEFAULT;
                ALTER TABLE "Permissions" ALTER COLUMN "IsAutoDiscovered" DROP DEFAULT;
                ALTER TABLE "Orders" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "OrderItems" ALTER COLUMN "IsDeleted" DROP DEFAULT;
                ALTER TABLE "AuditLogs" ALTER COLUMN "IsSuccess" DROP DEFAULT;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "UserRoles" ALTER COLUMN "RoleId" TYPE text USING "RoleId"::text;
                ALTER TABLE "UserRoles" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "TenantMemberships" ALTER COLUMN "IsActive" TYPE integer USING CASE WHEN "IsActive" THEN 1 ELSE 0 END;
                ALTER TABLE "TenantMemberships" ALTER COLUMN "CreatedAtUtc" TYPE text USING "CreatedAtUtc"::text;
                ALTER TABLE "TenantMemberships" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsDeleted" TYPE integer USING CASE WHEN "IsDeleted" THEN 1 ELSE 0 END;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsActive" TYPE integer USING CASE WHEN "IsActive" THEN 1 ELSE 0 END;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text;
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "Roles" ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text;
                ALTER TABLE "Roles" ALTER COLUMN "IsSystemRole" TYPE integer USING CASE WHEN "IsSystemRole" THEN 1 ELSE 0 END;
                ALTER TABLE "Roles" ALTER COLUMN "IsDeleted" TYPE integer USING CASE WHEN "IsDeleted" THEN 1 ELSE 0 END;
                ALTER TABLE "Roles" ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
                ALTER TABLE "Roles" ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text;
                ALTER TABLE "Roles" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "RolePermissions" ALTER COLUMN "RoleId" TYPE text USING "RoleId"::text;
                ALTER TABLE "RolePermissions" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsSystemDefault" TYPE integer USING CASE WHEN "IsSystemDefault" THEN 1 ELSE 0 END;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsDeleted" TYPE integer USING CASE WHEN "IsDeleted" THEN 1 ELSE 0 END;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsActive" TYPE integer USING CASE WHEN "IsActive" THEN 1 ELSE 0 END;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsDeleted" TYPE integer USING CASE WHEN "IsDeleted" THEN 1 ELSE 0 END;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsActive" TYPE integer USING CASE WHEN "IsActive" THEN 1 ELSE 0 END;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "ProcessedOnUtc" TYPE text USING "ProcessedOnUtc"::text;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "MessageId" TYPE text USING "MessageId"::text;
                ALTER TABLE "ProcessedMessages" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "Permissions" ALTER COLUMN "IsSystem" TYPE integer USING CASE WHEN "IsSystem" THEN 1 ELSE 0 END;
                ALTER TABLE "Permissions" ALTER COLUMN "IsAutoDiscovered" TYPE integer USING CASE WHEN "IsAutoDiscovered" THEN 1 ELSE 0 END;
                ALTER TABLE "Permissions" ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text;
                ALTER TABLE "Permissions" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "ProcessedOnUtc" TYPE text USING "ProcessedOnUtc"::text;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "OccurredOnUtc" TYPE text USING "OccurredOnUtc"::text;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "NextAttemptAtUtc" TYPE text USING "NextAttemptAtUtc"::text;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "LockedUntilUtc" TYPE text USING "LockedUntilUtc"::text;
                ALTER TABLE "OutboxMessages" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "Orders" ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text;
                ALTER TABLE "Orders" ALTER COLUMN "TotalAmount" TYPE text USING "TotalAmount"::text;
                ALTER TABLE "Orders" ALTER COLUMN "IsDeleted" TYPE integer USING CASE WHEN "IsDeleted" THEN 1 ELSE 0 END;
                ALTER TABLE "Orders" ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
                ALTER TABLE "Orders" ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text;
                ALTER TABLE "Orders" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "OrderItems" ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text;
                ALTER TABLE "OrderItems" ALTER COLUMN "UnitPrice" TYPE text USING "UnitPrice"::text;
                ALTER TABLE "OrderItems" ALTER COLUMN "Total" TYPE text USING "Total"::text;
                ALTER TABLE "OrderItems" ALTER COLUMN "OrderId" TYPE text USING "OrderId"::text;
                ALTER TABLE "OrderItems" ALTER COLUMN "IsDeleted" TYPE integer USING CASE WHEN "IsDeleted" THEN 1 ELSE 0 END;
                ALTER TABLE "OrderItems" ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
                ALTER TABLE "OrderItems" ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text;
                ALTER TABLE "OrderItems" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "ExpiresAtUtc" TYPE text USING "ExpiresAtUtc"::text;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "CreatedAtUtc" TYPE text USING "CreatedAtUtc"::text;
                ALTER TABLE "HttpIdempotencyRecords" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                ALTER TABLE "AuditLogs" ALTER COLUMN "Timestamp" TYPE text USING "Timestamp"::text;
                ALTER TABLE "AuditLogs" ALTER COLUMN "IsSuccess" TYPE integer USING CASE WHEN "IsSuccess" THEN 1 ELSE 0 END;
                ALTER TABLE "AuditLogs" ALTER COLUMN "DurationMs" TYPE integer USING "DurationMs"::integer;
                ALTER TABLE "AuditLogs" ALTER COLUMN "Id" TYPE text USING "Id"::text;
                """);

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptAtUtc_LockedUntilU~",
                table: "OutboxMessages",
                newName: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptAtUtc_LockedUntilUtc");

            migrationBuilder.RenameIndex(
                name: "IX_HttpIdempotencyRecords_TenantId_UserId_Method_Path_Idempote~",
                table: "HttpIdempotencyRecords",
                newName: "IX_HttpIdempotencyRecords_TenantId_UserId_Method_Path_IdempotencyKey");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserRoles",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "UserRoles",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "UserRoles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "UserRoles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TenantMemberships",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "TenantMemberships",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "TenantMemberships",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAtUtc",
                table: "TenantMemberships",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "TenantMemberships",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedAt",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SemanticDatasets",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "SemanticDatasets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "SemanticDatasets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "FieldsMetadataJson",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SemanticDatasets",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedAt",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "SemanticDatasets",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "SemanticDatasets",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "BaseQuerySql",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "SemanticDatasets",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Roles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedAt",
                table: "Roles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Roles",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "IsSystemRole",
                table: "Roles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "Roles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "Roles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedAt",
                table: "Roles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Roles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "Roles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Roles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "RolePermissions",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "RolePermissions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "PermissionCode",
                table: "RolePermissions",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "RolePermissions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "ReportTemplates",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "ReportTemplates",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedAt",
                table: "ReportTemplates",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "ReportTemplates",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ReportTemplates",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "IsSystemDefault",
                table: "ReportTemplates",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "ReportTemplates",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "ReportTemplates",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ReportTemplates",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "ReportTemplates",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedAt",
                table: "ReportTemplates",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ReportTemplates",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "ReportTemplates",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ReportTemplates",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ReportTemplates",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ReportTemplates",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ReportTemplates",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "ReportConfigurations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedAt",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "ReportConfigurations",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TemplateContent",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SelectedFieldsJson",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ReportConfigurations",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "ReportConfigurations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "ReportConfigurations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "FilterConfigJson",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedAt",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DatasetCode",
                table: "ReportConfigurations",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ReportConfigurations",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ReportConfigurations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedOnUtc",
                table: "ProcessedMessages",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "MessageId",
                table: "ProcessedMessages",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "ConsumerName",
                table: "ProcessedMessages",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ProcessedMessages",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Resource",
                table: "Permissions",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Module",
                table: "Permissions",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "IsSystem",
                table: "Permissions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "IsAutoDiscovered",
                table: "Permissions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "Permissions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Permissions",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "Permissions",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Permissions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "OutboxMessages",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "RetryCount",
                table: "OutboxMessages",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedOnUtc",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OccurredOnUtc",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MessageType",
                table: "OutboxMessages",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "LockedUntilUtc",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LockOwner",
                table: "OutboxMessages",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Error",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "OutboxMessages",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "OutboxMessages",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Orders",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedAt",
                table: "Orders",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TotalAmount",
                table: "Orders",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Orders",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "Orders",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedAt",
                table: "Orders",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Orders",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedAt",
                table: "OrderItems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UnitPrice",
                table: "OrderItems",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Total",
                table: "OrderItems",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "OrderItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "OrderItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "OrderItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedAt",
                table: "OrderItems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "OrderItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "OrderItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "ResponseStatusCode",
                table: "HttpIdempotencyRecords",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponseContentType",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponseBody",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RequestHash",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ExpiresAtUtc",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAtUtc",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "HttpIdempotencyRecords",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Timestamp",
                table: "AuditLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "IsSuccess",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "DurationMs",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AuditLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.Sql("""
                ALTER TABLE "SemanticDatasets" ALTER COLUMN "IsDeleted" SET DEFAULT 0;
                ALTER TABLE "Roles" ALTER COLUMN "IsDeleted" SET DEFAULT 0;
                ALTER TABLE "ReportTemplates" ALTER COLUMN "IsDeleted" SET DEFAULT 0;
                ALTER TABLE "ReportConfigurations" ALTER COLUMN "IsDeleted" SET DEFAULT 0;
                ALTER TABLE "Orders" ALTER COLUMN "IsDeleted" SET DEFAULT 0;
                ALTER TABLE "OrderItems" ALTER COLUMN "IsDeleted" SET DEFAULT 0;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
