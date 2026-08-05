using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHub.Persistence.Migrations
{
    public partial class AddStandardScheduleColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TieuDe",
                table: "LichHoc",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TieuDe",
                table: "LichThi",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GiangVien",
                table: "LichThi",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaMonHoc",
                table: "SuKien",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GiangVien",
                table: "SuKien",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HinhThucThi",
                table: "SuKien",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuKien_MaMonHoc",
                table: "SuKien",
                column: "MaMonHoc");

            migrationBuilder.AddForeignKey(
                name: "FK_SuKien_MonHoc_MaMonHoc",
                table: "SuKien",
                column: "MaMonHoc",
                principalTable: "MonHoc",
                principalColumn: "MaMonHoc",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuKien_MonHoc_MaMonHoc",
                table: "SuKien");

            migrationBuilder.DropIndex(
                name: "IX_SuKien_MaMonHoc",
                table: "SuKien");

            migrationBuilder.DropColumn(
                name: "TieuDe",
                table: "LichHoc");

            migrationBuilder.DropColumn(
                name: "TieuDe",
                table: "LichThi");

            migrationBuilder.DropColumn(
                name: "GiangVien",
                table: "LichThi");

            migrationBuilder.DropColumn(
                name: "MaMonHoc",
                table: "SuKien");

            migrationBuilder.DropColumn(
                name: "GiangVien",
                table: "SuKien");

            migrationBuilder.DropColumn(
                name: "HinhThucThi",
                table: "SuKien");
        }
    }
}
