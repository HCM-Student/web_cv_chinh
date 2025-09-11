using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CV.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishColumnsToBaiViet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayGui",
                table: "LienHes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "NoiDungEn",
                table: "BaiViets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TieuDeEn",
                table: "BaiViets",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TomTatEn",
                table: "BaiViets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LienHes_NgayGui",
                table: "LienHes",
                column: "NgayGui");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LienHes_NgayGui",
                table: "LienHes");

            migrationBuilder.DropColumn(
                name: "NoiDungEn",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "TieuDeEn",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "TomTatEn",
                table: "BaiViets");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayGui",
                table: "LienHes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");
        }
    }
}
