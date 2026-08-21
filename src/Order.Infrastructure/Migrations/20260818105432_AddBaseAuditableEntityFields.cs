using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolverineApp.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseAuditableEntityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Orders",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Orders",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Orders",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OrderItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "OrderItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "OrderItems");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
