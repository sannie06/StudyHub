using System;

namespace StudyHub.Application.DTOs.User
{
    public class UserProfileDto
    {
        public int MaNguoiDung { get; set; }
        public string Email { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public byte? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
        public string? AnhDaiDien { get; set; }
        public string VaiTro { get; set; } = null!;
    }
}
