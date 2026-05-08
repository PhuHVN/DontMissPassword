using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DontMissPassword.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addDateItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VaultItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VaultItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VaultItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VaultItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VaultItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VaultItems");
        }
    }
}
