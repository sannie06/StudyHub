namespace StudyHub.Application.DTOs.Auth
{
    public class UserDto
    {
        public int MaNguoiDung { get; set; }
        public string Email { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string VaiTro { get; set; } = null!;
    }
}
