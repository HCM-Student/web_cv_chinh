using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBinhLuanModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NoiDung",
                table: "BinhLuans",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "DaDuyet",
                table: "BinhLuans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "BinhLuans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HoTen",
                table: "BinhLuans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NguoiDungId",
                table: "BinhLuans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuans_NguoiDungId",
                table: "BinhLuans",
                column: "NguoiDungId");

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuans_NguoiDungs_NguoiDungId",
                table: "BinhLuans",
                column: "NguoiDungId",
                principalTable: "NguoiDungs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuans_NguoiDungs_NguoiDungId",
                table: "BinhLuans");

            migrationBuilder.DropIndex(
                name: "IX_BinhLuans_NguoiDungId",
                table: "BinhLuans");

            migrationBuilder.DropColumn(
                name: "DaDuyet",
                table: "BinhLuans");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "BinhLuans");

            migrationBuilder.DropColumn(
                name: "HoTen",
                table: "BinhLuans");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "BinhLuans");

            migrationBuilder.AlterColumn<string>(
                name: "NoiDung",
                table: "BinhLuans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
