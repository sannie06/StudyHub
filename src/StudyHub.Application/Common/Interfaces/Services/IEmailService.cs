using System.Threading.Tasks;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendConfirmationEmailAsync(string toEmail, string hoTen, string confirmLink);
        Task SendOtpEmailAsync(string toEmail, string hoTen, string otpCode);
        Task SendPasswordResetOtpEmailAsync(string toEmail, string hoTen, string otpCode);
    }
}
