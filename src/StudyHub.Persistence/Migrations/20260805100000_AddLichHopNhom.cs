using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace StudyHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLichHopNhom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LichHopNhom",
                columns: table => new
                {
                    MaLichHop = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhom = table.Column<int>(type: "int", nullable: false),
                    MaNguoiTao = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NenTang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHopNhom", x => x.MaLichHop);
                    table.ForeignKey(
                        name: "FK_LichHopNhom_NhomHocTap_MaNhom",
                        column: x => x.MaNhom,
                        principalTable: "NhomHocTap",
                        principalColumn: "MaNhom",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichHopNhom_NguoiDung_MaNguoiTao",
                        column: x => x.MaNguoiTao,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LichHopNhom_MaNhom",
                table: "LichHopNhom",
                column: "MaNhom");

            migrationBuilder.CreateIndex(
                name: "IX_LichHopNhom_MaNguoiTao",
                table: "LichHopNhom",
                column: "MaNguoiTao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichHopNhom");
        }
    }
}
