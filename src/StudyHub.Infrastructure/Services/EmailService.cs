using System;
using System.Net;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Infrastructure.Security;

namespace StudyHub.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<SmtpSettings> smtpSettingsOption,
            ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettingsOption.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_smtpSettings.Email) || string.IsNullOrWhiteSpace(_smtpSettings.Password))
            {
                _logger.LogWarning("SMTP Settings email or password is empty. Email sending skipped for {ToEmail}.", toEmail);
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"[DEBUG SMTP] Email: '{_smtpSettings.Email}' | Password: '{_smtpSettings.Password}'");

                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress("StudyHub Notification", _smtpSettings.Email));
                    emailMessage.To.Add(MailboxAddress.Parse(toEmail));
                    emailMessage.Subject = subject;

                    var bodyBuilder = new BodyBuilder
                    {
                        HtmlBody = htmlBody
                    };
                    emailMessage.Body = bodyBuilder.ToMessageBody();

                    using var client = new SmtpClient();
                    // Connect to Gmail SMTP using STARTTLS (port 587) or SSL (port 465)
                    var secureOption = _smtpSettings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                    await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, secureOption);

                    var cleanPassword = _smtpSettings.Password?.Trim()?.Replace(" ", "");
                    await client.AuthenticateAsync(_smtpSettings.Email, cleanPassword);

                    Console.WriteLine($"[SMTP EMAIL] Sending email via MailKit to {toEmail}...");
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);

                    Console.WriteLine($"[SMTP EMAIL SUCCESS] Email sent successfully via MailKit to {toEmail}!");
                    _logger.LogInformation("Email sent successfully via MailKit to {ToEmail} with subject: {Subject}", toEmail, subject);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SMTP EMAIL ERROR] MailKit failed to send email to {toEmail}: {ex.Message}");
                    _logger.LogError(ex, "MailKit failed to send email to {ToEmail}. Subject: {Subject}", toEmail, subject);
                }
            });

            await Task.CompletedTask;
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string hoTen, string confirmLink)
        {
            var subject = "[StudyHub] Xác thực địa chỉ Email của bạn";
            var htmlBody = $"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>
<body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f5ff; margin: 0; padding: 30px 15px;">
    <div style="max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 28px; padding: 40px 36px; box-shadow: 0 15px 35px rgba(91, 77, 255, 0.06); border: 1px solid #eef0ff; text-align: center;">
        
        <!-- Header Logo -->
        <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="margin: 0 auto 28px auto;">
            <tr>
                <td style="vertical-align: middle;">
                    <div style="background-color: #5B4DFF; border-radius: 14px; width: 44px; height: 44px; line-height: 44px; text-align: center; color: #ffffff; font-size: 22px; box-shadow: 0 6px 16px rgba(91, 77, 255, 0.25);">
                        🎓
                    </div>
                </td>
                <td style="vertical-align: middle; padding-left: 12px;">
                    <span style="font-size: 28px; font-weight: 800; color: #111827; letter-spacing: -0.5px;">Study<span style="color: #5B4DFF;">Hub</span></span>
                </td>
            </tr>
        </table>

        <!-- Heading -->
        <h1 style="font-size: 26px; font-weight: 800; color: #111827; margin: 0 0 12px 0; letter-spacing: -0.3px;">
            Chào mừng <span style="color: #5B4DFF;">{WebUtility.HtmlEncode(hoTen)}</span>!
        </h1>

        <!-- Context Description -->
        <p style="color: #4B5563; font-size: 15px; line-height: 1.6; margin: 0 0 28px 0;">
            Cảm ơn bạn đã đăng ký tài khoản tại <strong style="color: #5B4DFF;">StudyHub</strong>. Vui lòng nhấn vào nút bên dưới để xác thực địa chỉ email và kích hoạt tài khoản của bạn:
        </p>

        <!-- CTA Button -->
        <div style="margin: 28px 0;">
            <a href="{confirmLink}" target="_blank" style="display: inline-block; background: linear-gradient(135deg, #5B4DFF 0%, #6366F1 100%); color: #ffffff !important; text-decoration: none; padding: 16px 36px; border-radius: 18px; font-weight: 700; font-size: 15px; box-shadow: 0 8px 20px rgba(91, 77, 255, 0.3); transition: all 0.2s;">
                Kích hoạt tài khoản ngay &rarr;
            </a>
        </div>

        <p style="color: #6B7280; font-size: 13px; line-height: 1.5; margin: 0 0 32px 0;">
            Nếu nút trên không hoạt động, bạn có thể sao chép liên kết sau và dán vào trình duyệt:<br/>
            <a href="{confirmLink}" style="color: #5B4DFF; word-break: break-all; font-weight: 500;">{confirmLink}</a>
        </p>

        <!-- Footer -->
        <div style="border-top: 1px solid #F3F4F6; padding-top: 20px; font-size: 12px; color: #9CA3AF;">
            &copy; 2026 StudyHub Platform. Tất cả quyền được bảo lưu.
        </div>

    </div>
</body>
</html>
""";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendOtpEmailAsync(string toEmail, string hoTen, string otpCode)
        {
            var subject = $"[{otpCode}] Mã xác thực tài khoản StudyHub của bạn";
            var htmlBody = $"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>
<body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f5ff; margin: 0; padding: 30px 15px;">
    <div style="max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 28px; padding: 40px 36px; box-shadow: 0 15px 35px rgba(91, 77, 255, 0.06); border: 1px solid #eef0ff; text-align: center; position: relative; overflow: hidden;">
        
        <!-- Header Logo -->
        <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="margin: 0 auto 28px auto;">
            <tr>
                <td style="vertical-align: middle;">
                    <div style="background-color: #5B4DFF; border-radius: 14px; width: 44px; height: 44px; line-height: 44px; text-align: center; color: #ffffff; font-size: 22px; box-shadow: 0 6px 16px rgba(91, 77, 255, 0.25);">
                        🎓
                    </div>
                </td>
                <td style="vertical-align: middle; padding-left: 12px;">
                    <span style="font-size: 28px; font-weight: 800; color: #111827; letter-spacing: -0.5px;">Study<span style="color: #5B4DFF;">Hub</span></span>
                </td>
            </tr>
        </table>

        <!-- Heading Salutation -->
        <h1 style="font-size: 26px; font-weight: 800; color: #111827; margin: 0 0 12px 0; letter-spacing: -0.3px;">
            Xin chào <span style="color: #5B4DFF;">{WebUtility.HtmlEncode(hoTen)}</span>,
        </h1>

        <!-- Context Description -->
        <p style="color: #4B5563; font-size: 15px; line-height: 1.6; margin: 0 0 6px 0;">
            Bạn vừa yêu cầu kích hoạt tài khoản <strong style="color: #5B4DFF;">StudyHub</strong>.
        </p>
        <p style="color: #6B7280; font-size: 14px; margin: 0 0 20px 0;">
            Đây là mã xác thực OTP của bạn:
        </p>

        <!-- 6-Digit OTP Box -->
        <div style="background-color: #F4F5FF; border: 1.5px solid #E0E7FF; border-radius: 24px; padding: 22px 32px; display: inline-block; margin: 0 0 16px 0; box-shadow: 0 8px 24px rgba(91, 77, 255, 0.08);">
            <span style="color: #5B4DFF; font-size: 38px; font-weight: 900; letter-spacing: 16px; font-family: 'Courier New', Courier, monospace; padding-left: 16px;">{otpCode}</span>
        </div>

        <!-- Expiration Note -->
        <p style="color: #6B7280; font-size: 13px; margin: 0 0 28px 0;">
            Mã có hiệu lực trong <strong style="color: #5B4DFF;">5 phút</strong>
        </p>

        <!-- Security Notice Card (Modern Soft Purple Gradient Glassmorphic Badge) -->
        <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="width: 100%; background-color: #F8F9FE; border: 1px solid #EEF0FF; border-radius: 20px; padding: 20px 24px; text-align: left; margin-bottom: 28px;">
            <tr>
                <td style="vertical-align: middle; width: 44px; padding-right: 16px;">
                    <div style="background-color: #EEF0FF; border-radius: 50%; width: 44px; height: 44px; line-height: 44px; text-align: center; font-size: 22px; display: inline-block; box-shadow: 0 4px 12px rgba(91, 77, 255, 0.12); border: 1px solid #E0E7FF;">
                        🛡️
                    </div>
                </td>
                <!-- Middle Text Content -->
                <td style="vertical-align: middle;">
                    <h4 style="margin: 0 0 4px 0; font-size: 14px; font-weight: 700; color: #111827;">Lưu ý bảo mật</h4>
                    <p style="margin: 0; font-size: 12px; color: #6B7280; line-height: 1.55;">
                        Không chia sẻ mã này với bất kỳ ai. StudyHub sẽ không bao giờ yêu cầu bạn cung cấp mã OTP qua email hoặc điện thoại.
                    </p>
                </td>
            </tr>
        </table>

        <!-- Help Notice with Sleek Purple (?) Badge -->
        <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="margin: 0 auto 24px auto;">
            <tr>
                <td style="vertical-align: middle; padding-right: 8px;">
                    <div style="background-color: #EEF0FF; color: #5B4DFF; border-radius: 50%; width: 24px; height: 24px; line-height: 24px; text-align: center; font-size: 13px; font-weight: 800; border: 1px solid #C7D2FE; display: inline-block; box-shadow: 0 2px 8px rgba(91, 77, 255, 0.12);">?</div>
                </td>
                <td style="vertical-align: middle; text-align: left;">
                    <span style="color: #6B7280; font-size: 12px; line-height: 1.5;">
                        Nếu bạn không yêu cầu mã này, hãy bỏ qua email này hoặc <a href="#" style="color: #5B4DFF; font-weight: 600; text-decoration: none;">liên hệ hỗ trợ</a> nếu cần giúp đỡ.
                    </span>
                </td>
            </tr>
        </table>

        <!-- Footer Links & Copyright -->
        <div style="border-top: 1px solid #F3F4F6; padding-top: 20px; font-size: 12px; color: #9CA3AF;">
            &copy; 2026 StudyHub Platform. Tất cả quyền được bảo lưu.
        </div>

    </div>
</body>
</html>
""";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendPasswordResetOtpEmailAsync(string toEmail, string hoTen, string otpCode)
        {
            var subject = $"[{otpCode}] Mã OTP khôi phục mật khẩu StudyHub của bạn";
            var htmlBody = $"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>
<body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f5ff; margin: 0; padding: 30px 15px;">
    <div style="max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 28px; padding: 40px 36px; box-shadow: 0 15px 35px rgba(91, 77, 255, 0.06); border: 1px solid #eef0ff; text-align: center; position: relative; overflow: hidden;">
        
        <!-- Header Logo -->
        <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="margin: 0 auto 28px auto;">
            <tr>
                <td style="vertical-align: middle;">
                    <div style="background-color: #5B4DFF; border-radius: 14px; width: 44px; height: 44px; line-height: 44px; text-align: center; color: #ffffff; font-size: 22px; box-shadow: 0 6px 16px rgba(91, 77, 255, 0.25);">
                        🔑
                    </div>
                </td>
                <td style="vertical-align: middle; padding-left: 12px;">
                    <span style="font-size: 28px; font-weight: 800; color: #111827; letter-spacing: -0.5px;">Study<span style="color: #5B4DFF;">Hub</span></span>
                </td>
            </tr>
        </table>

        <!-- Heading Salutation -->
        <h1 style="font-size: 26px; font-weight: 800; color: #111827; margin: 0 0 12px 0; letter-spacing: -0.3px;">
            Xin chào <span style="color: #5B4DFF;">{WebUtility.HtmlEncode(hoTen)}</span>,
        </h1>

        <!-- Context Description -->
        <p style="color: #4B5563; font-size: 15px; line-height: 1.6; margin: 0 0 6px 0;">
            Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản <strong style="color: #5B4DFF;">StudyHub</strong>.
        </p>
        <p style="color: #6B7280; font-size: 14px; margin: 0 0 20px 0;">
            Mã OTP khôi phục mật khẩu của bạn là:
        </p>

        <!-- 6-Digit OTP Box -->
        <div style="background-color: #FFF5F5; border: 1.5px solid #FEE2E2; border-radius: 24px; padding: 22px 32px; display: inline-block; margin: 0 0 16px 0; box-shadow: 0 8px 24px rgba(239, 68, 68, 0.08);">
            <span style="color: #EF4444; font-size: 38px; font-weight: 900; letter-spacing: 16px; font-family: 'Courier New', Courier, monospace; padding-left: 16px;">{otpCode}</span>
        </div>

        <!-- Expiration Note -->
        <p style="color: #6B7280; font-size: 13px; margin: 0 0 28px 0;">
            Mã có hiệu lực trong <strong style="color: #EF4444;">5 phút</strong>
        </p>

        <!-- Security Notice Card (Modern Soft Red Gradient Glassmorphic Badge) -->
        <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="width: 100%; background-color: #F8F9FE; border: 1px solid #EEF0FF; border-radius: 20px; padding: 20px 24px; text-align: left; margin-bottom: 28px;">
            <tr>
                <td style="vertical-align: middle; width: 44px; padding-right: 16px;">
                    <div style="background-color: #FFF5F5; border-radius: 50%; width: 44px; height: 44px; line-height: 44px; text-align: center; font-size: 22px; display: inline-block; box-shadow: 0 4px 12px rgba(239, 68, 68, 0.12); border: 1px solid #FEE2E2;">
                        🛡️
                    </div>
                </td>
                <!-- Middle Text Content -->
                <td style="vertical-align: middle;">
                    <h4 style="margin: 0 0 4px 0; font-size: 14px; font-weight: 700; color: #111827;">Lưu ý bảo mật</h4>
                    <p style="margin: 0; font-size: 12px; color: #6B7280; line-height: 1.55;">
                        Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này và đổi mật khẩu tài khoản lập tức.
                    </p>
                </td>
            </tr>
        </table>

        <!-- Help Notice with Sleek Purple (?) Badge -->
        <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="margin: 0 auto 24px auto;">
            <tr>
                <td style="vertical-align: middle; padding-right: 8px;">
                    <div style="background-color: #EEF0FF; color: #5B4DFF; border-radius: 50%; width: 24px; height: 24px; line-height: 24px; text-align: center; font-size: 13px; font-weight: 800; border: 1px solid #C7D2FE; display: inline-block; box-shadow: 0 2px 8px rgba(91, 77, 255, 0.12);">?</div>
                </td>
                <td style="vertical-align: middle; text-align: left;">
                    <span style="color: #6B7280; font-size: 12px; line-height: 1.5;">
                        Cần hỗ trợ khẩn cấp? <a href="#" style="color: #5B4DFF; font-weight: 600; text-decoration: none;">Liên hệ đội ngũ hỗ trợ StudyHub</a>
                    </span>
                </td>
            </tr>
        </table>

        <!-- Footer -->
        <div style="border-top: 1px solid #F3F4F6; padding-top: 20px; font-size: 12px; color: #9CA3AF;">
            &copy; 2026 StudyHub Platform. Tất cả quyền được bảo lưu.
        </div>

    </div>
</body>
</html>
""";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }
    }
}
