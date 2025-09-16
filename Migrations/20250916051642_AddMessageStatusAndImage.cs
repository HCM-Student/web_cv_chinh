using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CV.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageStatusAndImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ChatMessages",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ChatMessages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SeenAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ChatMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "SeenAt",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ChatMessages",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
