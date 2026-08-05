using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudyHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauHinhHeThong",
                columns: table => new
                {
                    MaCauHinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenCauHinh = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GiaTri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhHeThong", x => x.MaCauHinh);
                });

            migrationBuilder.CreateTable(
                name: "LoaiThongBao",
                columns: table => new
                {
                    MaLoaiThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiThongBao", x => x.MaLoaiThongBao);
                });

            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    MaMonHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMonHoc = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaMon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc", x => x.MaMonHoc);
                });

            migrationBuilder.CreateTable(
                name: "Quyen",
                columns: table => new
                {
                    MaQuyen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenQuyen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quyen", x => x.MaQuyen);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MatKhauHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<byte>(type: "tinyint", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    LanDangNhapCuoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NguoiCapNhat = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.MaNguoiDung);
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_MaVaiTro",
                        column: x => x.MaVaiTro,
                        principalTable: "VaiTro",
                        principalColumn: "MaVaiTro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileTaiLen",
                columns: table => new
                {
                    MaFile = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    TenGoc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenLuu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LoaiFile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DungLuong = table.Column<long>(type: "bigint", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayTaiLen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTaiLen", x => x.MaFile);
                    table.ForeignKey(
                        name: "FK_FileTaiLen_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoiThoaiAI",
                columns: table => new
                {
                    MaHoiThoai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LoaiHoiThoai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoiThoaiAI", x => x.MaHoiThoai);
                    table.ForeignKey(
                        name: "FK_HoiThoaiAI_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KanbanBoard",
                columns: table => new
                {
                    MaBoard = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    TenBoard = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MacDinh = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanBoard", x => x.MaBoard);
                    table.ForeignKey(
                        name: "FK_KanbanBoard_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichHoc",
                columns: table => new
                {
                    MaLichHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaMonHoc = table.Column<int>(type: "int", nullable: false),
                    Thu = table.Column<byte>(type: "tinyint", nullable: false),
                    TietBatDau = table.Column<byte>(type: "tinyint", nullable: false),
                    TietKetThuc = table.Column<byte>(type: "tinyint", nullable: false),
                    PhongHoc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiangVien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHoc", x => x.MaLichHoc);
                    table.ForeignKey(
                        name: "FK_LichHoc_MonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalTable: "MonHoc",
                        principalColumn: "MaMonHoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichHoc_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuQuiz",
                columns: table => new
                {
                    MaQuiz = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    ChuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SoCauHoi = table.Column<int>(type: "int", nullable: false),
                    DiemSo = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuQuiz", x => x.MaQuiz);
                    table.ForeignKey(
                        name: "FK_LichSuQuiz_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichThi",
                columns: table => new
                {
                    MaLichThi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaMonHoc = table.Column<int>(type: "int", nullable: false),
                    HinhThucThi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayThi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiLuong = table.Column<int>(type: "int", nullable: true),
                    PhongThi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichThi", x => x.MaLichThi);
                    table.ForeignKey(
                        name: "FK_LichThi_MonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalTable: "MonHoc",
                        principalColumn: "MaMonHoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichThi_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHeThong",
                columns: table => new
                {
                    MaLog = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DiaChiIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrinhDuyet = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MucDo = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHeThong", x => x.MaLog);
                    table.ForeignKey(
                        name: "FK_NhatKyHeThong_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhomCongViec",
                columns: table => new
                {
                    MaNhomCongViec = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    TenNhom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NguoiCapNhat = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhomCongViec", x => x.MaNhomCongViec);
                    table.ForeignKey(
                        name: "FK_NhomCongViec_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhomHocTap",
                columns: table => new
                {
                    MaNhom = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiTao = table.Column<int>(type: "int", nullable: false),
                    MaMonHoc = table.Column<int>(type: "int", nullable: true),
                    TenNhom = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MaThamGia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SoLuongToiDa = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhomHocTap", x => x.MaNhom);
                    table.ForeignKey(
                        name: "FK_NhomHocTap_MonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalTable: "MonHoc",
                        principalColumn: "MaMonHoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NhomHocTap_NguoiDung_MaNguoiTao",
                        column: x => x.MaNguoiTao,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhienDangNhap",
                columns: table => new
                {
                    MaPhien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DiaChiIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrinhDuyet = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThietBi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ViTri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThoiGianDangNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianHetHan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianDangXuat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienDangNhap", x => x.MaPhien);
                    table.ForeignKey(
                        name: "FK_PhienDangNhap_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuKien",
                columns: table => new
                {
                    MaSuKien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NhacTruoc = table.Column<int>(type: "int", nullable: true),
                    LapLai = table.Column<bool>(type: "bit", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuKien", x => x.MaSuKien);
                    table.ForeignKey(
                        name: "FK_SuKien_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    MaThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaLoaiThongBao = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false),
                    MucDo = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDoc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.MaThongBao);
                    table.ForeignKey(
                        name: "FK_ThongBao_LoaiThongBao_MaLoaiThongBao",
                        column: x => x.MaLoaiThongBao,
                        principalTable: "LoaiThongBao",
                        principalColumn: "MaLoaiThongBao",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThongBao_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThongKeHocTap",
                columns: table => new
                {
                    MaThongKe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    TongCongViec = table.Column<int>(type: "int", nullable: false),
                    CongViecHoanThanh = table.Column<int>(type: "int", nullable: false),
                    CongViecQuaHan = table.Column<int>(type: "int", nullable: false),
                    TongPomodoro = table.Column<int>(type: "int", nullable: false),
                    TongPhutHoc = table.Column<int>(type: "int", nullable: false),
                    SoNgayHocLienTiep = table.Column<int>(type: "int", nullable: false),
                    TyLeHoanThanh = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiemNangSuat = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    NgayThongKe = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongKeHocTap", x => x.MaThongKe);
                    table.ForeignKey(
                        name: "FK_ThongKeHocTap_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuTomTat",
                columns: table => new
                {
                    MaTomTat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaFile = table.Column<int>(type: "int", nullable: false),
                    NoiDungTomTat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTomTat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuTomTat", x => x.MaTomTat);
                    table.ForeignKey(
                        name: "FK_LichSuTomTat_FileTaiLen_MaFile",
                        column: x => x.MaFile,
                        principalTable: "FileTaiLen",
                        principalColumn: "MaFile",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuTomTat_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TinNhanAI",
                columns: table => new
                {
                    MaTinNhanAI = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoiThoai = table.Column<int>(type: "int", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenSuDung = table.Column<int>(type: "int", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhanAI", x => x.MaTinNhanAI);
                    table.ForeignKey(
                        name: "FK_TinNhanAI_HoiThoaiAI_MaHoiThoai",
                        column: x => x.MaHoiThoai,
                        principalTable: "HoiThoaiAI",
                        principalColumn: "MaHoiThoai",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KanbanCot",
                columns: table => new
                {
                    MaCot = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBoard = table.Column<int>(type: "int", nullable: false),
                    TenCot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    GioiHanThe = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanCot", x => x.MaCot);
                    table.ForeignKey(
                        name: "FK_KanbanCot_KanbanBoard_MaBoard",
                        column: x => x.MaBoard,
                        principalTable: "KanbanBoard",
                        principalColumn: "MaBoard",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CongViec",
                columns: table => new
                {
                    MaCongViec = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaMonHoc = table.Column<int>(type: "int", nullable: true),
                    MaNhomCongViec = table.Column<int>(type: "int", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoUuTien = table.Column<byte>(type: "tinyint", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HanHoanThanh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayHoanThanh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TiLeHoanThanh = table.Column<int>(type: "int", nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DanhDauQuanTrong = table.Column<bool>(type: "bit", nullable: false),
                    DanhDauYeuThich = table.Column<bool>(type: "bit", nullable: false),
                    LapLai = table.Column<bool>(type: "bit", nullable: false),
                    SoLanLap = table.Column<int>(type: "int", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NguoiCapNhat = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongViec", x => x.MaCongViec);
                    table.ForeignKey(
                        name: "FK_CongViec_MonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalTable: "MonHoc",
                        principalColumn: "MaMonHoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CongViec_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CongViec_NhomCongViec_MaNhomCongViec",
                        column: x => x.MaNhomCongViec,
                        principalTable: "NhomCongViec",
                        principalColumn: "MaNhomCongViec",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu",
                columns: table => new
                {
                    MaTaiLieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhom = table.Column<int>(type: "int", nullable: false),
                    MaNguoiTaiLen = table.Column<int>(type: "int", nullable: false),
                    MaFile = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LuotTai = table.Column<int>(type: "int", nullable: false),
                    NgayTaiLen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu", x => x.MaTaiLieu);
                    table.ForeignKey(
                        name: "FK_TaiLieu_FileTaiLen_MaFile",
                        column: x => x.MaFile,
                        principalTable: "FileTaiLen",
                        principalColumn: "MaFile",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_NguoiDung_MaNguoiTaiLen",
                        column: x => x.MaNguoiTaiLen,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiLieu_NhomHocTap_MaNhom",
                        column: x => x.MaNhom,
                        principalTable: "NhomHocTap",
                        principalColumn: "MaNhom",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienNhom",
                columns: table => new
                {
                    MaThanhVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhom = table.Column<int>(type: "int", nullable: false),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    VaiTro = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayThamGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienNhom", x => x.MaThanhVien);
                    table.ForeignKey(
                        name: "FK_ThanhVienNhom_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThanhVienNhom_NhomHocTap_MaNhom",
                        column: x => x.MaNhom,
                        principalTable: "NhomHocTap",
                        principalColumn: "MaNhom",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TinNhan",
                columns: table => new
                {
                    MaTinNhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhom = table.Column<int>(type: "int", nullable: false),
                    MaNguoiGui = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiTinNhan = table.Column<byte>(type: "tinyint", nullable: false),
                    DaChinhSua = table.Column<bool>(type: "bit", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayChinhSua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhan", x => x.MaTinNhan);
                    table.ForeignKey(
                        name: "FK_TinNhan_NguoiDung_MaNguoiGui",
                        column: x => x.MaNguoiGui,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinNhan_NhomHocTap_MaNhom",
                        column: x => x.MaNhom,
                        principalTable: "NhomHocTap",
                        principalColumn: "MaNhom",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KanbanThe",
                columns: table => new
                {
                    MaThe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCot = table.Column<int>(type: "int", nullable: false),
                    MaCongViec = table.Column<int>(type: "int", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanThe", x => x.MaThe);
                    table.ForeignKey(
                        name: "FK_KanbanThe_CongViec_MaCongViec",
                        column: x => x.MaCongViec,
                        principalTable: "CongViec",
                        principalColumn: "MaCongViec",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KanbanThe_KanbanCot_MaCot",
                        column: x => x.MaCot,
                        principalTable: "KanbanCot",
                        principalColumn: "MaCot",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PomodoroSession",
                columns: table => new
                {
                    MaSession = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaCongViec = table.Column<int>(type: "int", nullable: true),
                    MaMonHoc = table.Column<int>(type: "int", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LoaiSession = table.Column<byte>(type: "tinyint", nullable: false),
                    ThoiLuong = table.Column<int>(type: "int", nullable: false),
                    SoLanTamDung = table.Column<int>(type: "int", nullable: false),
                    TongThoiGianTamDung = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSession", x => x.MaSession);
                    table.ForeignKey(
                        name: "FK_PomodoroSession_CongViec_MaCongViec",
                        column: x => x.MaCongViec,
                        principalTable: "CongViec",
                        principalColumn: "MaCongViec",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PomodoroSession_MonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalTable: "MonHoc",
                        principalColumn: "MaMonHoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PomodoroSession_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TepDinhKemTinNhan",
                columns: table => new
                {
                    MaTep = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTinNhan = table.Column<int>(type: "int", nullable: false),
                    MaFile = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKemTinNhan", x => x.MaTep);
                    table.ForeignKey(
                        name: "FK_TepDinhKemTinNhan_FileTaiLen_MaFile",
                        column: x => x.MaFile,
                        principalTable: "FileTaiLen",
                        principalColumn: "MaFile",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TepDinhKemTinNhan_TinNhan_MaTinNhan",
                        column: x => x.MaTinNhan,
                        principalTable: "TinNhan",
                        principalColumn: "MaTinNhan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LoaiThongBao",
                columns: new[] { "MaLoaiThongBao", "Icon", "MauSac", "MoTa", "TenLoai" },
                values: new object[,]
                {
                    { 1, "bi bi-alarm", "text-danger", "Thong bao han chot cong viec", "Deadline" },
                    { 2, "bi bi-bell", "text-warning", "Nhac nho hoc tap, su kien", "Reminder" },
                    { 3, "bi bi-people", "text-primary", "Thong bao hoat dong nhom", "Group" },
                    { 4, "bi bi-shield-exclamation", "text-secondary", "Thong bao tu he thong", "System" },
                    { 5, "bi bi-cpu", "text-info", "Goi y va bao cao tu AI", "AI" }
                });

            migrationBuilder.InsertData(
                table: "VaiTro",
                columns: new[] { "MaVaiTro", "DaXoa", "MoTa", "NgayCapNhat", "NgayTao", "TenVaiTro" },
                values: new object[,]
                {
                    { 1, false, "Quan tri he thong", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Administrator" },
                    { 2, false, "Sinh vien / Nguoi dung", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Student" }
                });

            migrationBuilder.InsertData(
                table: "NguoiDung",
                columns: new[] { "MaNguoiDung", "AnhDaiDien", "DaXoa", "DiaChi", "Email", "GioiTinh", "HoTen", "LanDangNhapCuoi", "MaVaiTro", "MatKhauHash", "NgayCapNhat", "NgaySinh", "NgayTao", "NguoiCapNhat", "NguoiTao", "SoDienThoai", "TrangThai" },
                values: new object[] { 1, null, false, null, "admin@studyhub.com", null, "System Admin", null, 1, "AQAAAAIAAYagAAAAEO9gD1yVzH7qKqTjV+UomN+gI6s8D/H8lE3wFvC9W2vVw==", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "0123456789", (byte)1 });

            migrationBuilder.CreateIndex(
                name: "IX_CongViec_MaMonHoc",
                table: "CongViec",
                column: "MaMonHoc");

            migrationBuilder.CreateIndex(
                name: "IX_CongViec_MaNguoiDung",
                table: "CongViec",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_CongViec_MaNhomCongViec",
                table: "CongViec",
                column: "MaNhomCongViec");

            migrationBuilder.CreateIndex(
                name: "IX_FileTaiLen_MaNguoiDung",
                table: "FileTaiLen",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_HoiThoaiAI_MaNguoiDung",
                table: "HoiThoaiAI",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanBoard_MaNguoiDung",
                table: "KanbanBoard",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanCot_MaBoard",
                table: "KanbanCot",
                column: "MaBoard");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanThe_MaCongViec",
                table: "KanbanThe",
                column: "MaCongViec",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KanbanThe_MaCot",
                table: "KanbanThe",
                column: "MaCot");

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_MaMonHoc",
                table: "LichHoc",
                column: "MaMonHoc");

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_MaNguoiDung",
                table: "LichHoc",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuQuiz_MaNguoiDung",
                table: "LichSuQuiz",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTomTat_MaFile",
                table: "LichSuTomTat",
                column: "MaFile");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTomTat_MaNguoiDung",
                table: "LichSuTomTat",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LichThi_MaMonHoc",
                table: "LichThi",
                column: "MaMonHoc");

            migrationBuilder.CreateIndex(
                name: "IX_LichThi_MaNguoiDung",
                table: "LichThi",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LoaiThongBao_TenLoai",
                table: "LoaiThongBao",
                column: "TenLoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_Email",
                table: "NguoiDung",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_MaVaiTro",
                table: "NguoiDung",
                column: "MaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThong_MaNguoiDung",
                table: "NhatKyHeThong",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_NhomCongViec_MaNguoiDung",
                table: "NhomCongViec",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_NhomHocTap_MaMonHoc",
                table: "NhomHocTap",
                column: "MaMonHoc");

            migrationBuilder.CreateIndex(
                name: "IX_NhomHocTap_MaNguoiTao",
                table: "NhomHocTap",
                column: "MaNguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_NhomHocTap_MaThamGia",
                table: "NhomHocTap",
                column: "MaThamGia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhienDangNhap_MaNguoiDung",
                table: "PhienDangNhap",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSession_MaCongViec",
                table: "PomodoroSession",
                column: "MaCongViec");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSession_MaMonHoc",
                table: "PomodoroSession",
                column: "MaMonHoc");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSession_MaNguoiDung",
                table: "PomodoroSession",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_SuKien_MaNguoiDung",
                table: "SuKien",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_MaFile",
                table: "TaiLieu",
                column: "MaFile");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_MaNguoiTaiLen",
                table: "TaiLieu",
                column: "MaNguoiTaiLen");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_MaNhom",
                table: "TaiLieu",
                column: "MaNhom");

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKemTinNhan_MaFile",
                table: "TepDinhKemTinNhan",
                column: "MaFile");

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKemTinNhan_MaTinNhan",
                table: "TepDinhKemTinNhan",
                column: "MaTinNhan");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienNhom_MaNguoiDung",
                table: "ThanhVienNhom",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienNhom_MaNhom_MaNguoiDung",
                table: "ThanhVienNhom",
                columns: new[] { "MaNhom", "MaNguoiDung" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaLoaiThongBao",
                table: "ThongBao",
                column: "MaLoaiThongBao");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaNguoiDung",
                table: "ThongBao",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_ThongKeHocTap_MaNguoiDung_NgayThongKe",
                table: "ThongKeHocTap",
                columns: new[] { "MaNguoiDung", "NgayThongKe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_MaNguoiGui",
                table: "TinNhan",
                column: "MaNguoiGui");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_MaNhom",
                table: "TinNhan",
                column: "MaNhom");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhanAI_MaHoiThoai",
                table: "TinNhanAI",
                column: "MaHoiThoai");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_TenVaiTro",
                table: "VaiTro",
                column: "TenVaiTro",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhHeThong");

            migrationBuilder.DropTable(
                name: "KanbanThe");

            migrationBuilder.DropTable(
                name: "LichHoc");

            migrationBuilder.DropTable(
                name: "LichSuQuiz");

            migrationBuilder.DropTable(
                name: "LichSuTomTat");

            migrationBuilder.DropTable(
                name: "LichThi");

            migrationBuilder.DropTable(
                name: "NhatKyHeThong");

            migrationBuilder.DropTable(
                name: "PhienDangNhap");

            migrationBuilder.DropTable(
                name: "PomodoroSession");

            migrationBuilder.DropTable(
                name: "Quyen");

            migrationBuilder.DropTable(
                name: "SuKien");

            migrationBuilder.DropTable(
                name: "TaiLieu");

            migrationBuilder.DropTable(
                name: "TepDinhKemTinNhan");

            migrationBuilder.DropTable(
                name: "ThanhVienNhom");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "ThongKeHocTap");

            migrationBuilder.DropTable(
                name: "TinNhanAI");

            migrationBuilder.DropTable(
                name: "KanbanCot");

            migrationBuilder.DropTable(
                name: "CongViec");

            migrationBuilder.DropTable(
                name: "FileTaiLen");

            migrationBuilder.DropTable(
                name: "TinNhan");

            migrationBuilder.DropTable(
                name: "LoaiThongBao");

            migrationBuilder.DropTable(
                name: "HoiThoaiAI");

            migrationBuilder.DropTable(
                name: "KanbanBoard");

            migrationBuilder.DropTable(
                name: "NhomCongViec");

            migrationBuilder.DropTable(
                name: "NhomHocTap");

            migrationBuilder.DropTable(
                name: "MonHoc");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "VaiTro");
        }
    }
}
