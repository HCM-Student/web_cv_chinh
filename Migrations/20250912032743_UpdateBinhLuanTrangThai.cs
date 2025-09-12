using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBinhLuanTrangThai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaDuyet",
                table: "BinhLuans");

            migrationBuilder.AddColumn<int>(
                name: "TrangThai",
                table: "BinhLuans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "BinhLuans");

            migrationBuilder.AddColumn<bool>(
                name: "DaDuyet",
                table: "BinhLuans",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
