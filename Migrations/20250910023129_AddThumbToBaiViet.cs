using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_cv.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbToBaiViet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhTieuDe",
                table: "BaiViets",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnhTieuDeAlt",
                table: "BaiViets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnhTieuDe",
                table: "BaiViets");

            migrationBuilder.DropColumn(
                name: "AnhTieuDeAlt",
                table: "BaiViets");
        }
    }
}
