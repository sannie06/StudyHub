namespace StudyHub.Application.DTOs.Auth
{
    public class RegisterRequest
    {
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
    }
}
