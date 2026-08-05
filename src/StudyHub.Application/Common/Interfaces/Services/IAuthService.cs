using System.Threading.Tasks;
using StudyHub.Application.DTOs.Auth;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
        Task LogoutAsync(string refreshToken, string ipAddress);
        Task<bool> VerifyOtpAsync(VerifyOtpRequest request);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<bool> ResendOtpAsync(string email);
    }
}
