namespace StudyHub.Application.DTOs.Auth
{
    public class GoogleAuthRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string? AvatarUrl { get; set; }
        public string? GoogleId { get; set; }
        public string? Credential { get; set; }
    }
}
