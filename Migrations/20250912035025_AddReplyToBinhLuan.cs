using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CV.Migrations
{
    /// <inheritdoc />
    public partial class AddReplyToBinhLuan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "BinhLuans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuans_ParentId",
                table: "BinhLuans",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuans_BinhLuans_ParentId",
                table: "BinhLuans",
                column: "ParentId",
                principalTable: "BinhLuans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BinhLuans_BinhLuans_ParentId",
                table: "BinhLuans");

            migrationBuilder.DropIndex(
                name: "IX_BinhLuans_ParentId",
                table: "BinhLuans");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "BinhLuans");
        }
    }
}
