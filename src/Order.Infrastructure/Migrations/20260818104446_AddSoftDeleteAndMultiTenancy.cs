using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolverineApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Orders",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrderItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuditLogs");
        }
    }
}
