using System;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class ThanhVienNhomDto
    {
        public int MaThanhVien { get; set; }
        public int MaNhom { get; set; }
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public byte VaiTro { get; set; } // 0: Member, 1: Moderator, 2: Owner
        public byte TrangThai { get; set; } // 1: Active
        public DateTime NgayThamGia { get; set; }
    }
}
