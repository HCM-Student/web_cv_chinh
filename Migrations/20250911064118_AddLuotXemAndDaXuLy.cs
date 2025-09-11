using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CV.Migrations
{
    /// <inheritdoc />
    public partial class AddLuotXemAndDaXuLy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaXuLy",
                table: "LienHes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LuotXem",
                table: "BaiViets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaXuLy",
                table: "LienHes");

            migrationBuilder.DropColumn(
                name: "LuotXem",
                table: "BaiViets");
        }
    }
}
