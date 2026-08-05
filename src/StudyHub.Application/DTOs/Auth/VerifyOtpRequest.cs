namespace StudyHub.Application.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string LoaiOTP { get; set; } = null!; // Register, ForgotPassword
    }
}
