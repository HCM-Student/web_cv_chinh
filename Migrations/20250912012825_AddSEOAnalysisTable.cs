using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CV.Migrations
{
    /// <inheritdoc />
    public partial class AddSEOAnalysisTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SEOAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaiVietId = table.Column<int>(type: "int", nullable: false),
                    TongDiem = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiemTieuDe = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiemTomTat = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiemNoiDung = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiemTuKhoa = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiemCauTruc = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiemHinhAnh = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiemLienKet = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiemDoDai = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TieuDePhanTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TomTatPhanTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDungPhanTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuKhoaPhanTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CauTrucPhanTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnhPhanTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LienKetPhanTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoDaiPhanTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoiYCaiThien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayPhanTich = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    DaXuLy = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEOAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SEOAnalyses_BaiViets_BaiVietId",
                        column: x => x.BaiVietId,
                        principalTable: "BaiViets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SEOAnalyses_BaiVietId",
                table: "SEOAnalyses",
                column: "BaiVietId");

            migrationBuilder.CreateIndex(
                name: "IX_SEOAnalyses_NgayPhanTich",
                table: "SEOAnalyses",
                column: "NgayPhanTich");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SEOAnalyses");
        }
    }
}
