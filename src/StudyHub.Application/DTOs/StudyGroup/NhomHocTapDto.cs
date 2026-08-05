using System;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class NhomHocTapDto
    {
        public int MaNhom { get; set; }
        public int MaNguoiTao { get; set; }
        public string TenNguoiTao { get; set; } = string.Empty;
        public int? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public string TenNhom { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string AnhDaiDien { get; set; } = string.Empty;
        public string MaThamGia { get; set; } = string.Empty;
        public int SoLuongToiDa { get; set; }
        public int SoThanhVienHienTai { get; set; }
        public byte TrangThai { get; set; }
        public bool IsOwner { get; set; }
        public bool IsMember { get; set; }
    }
}
