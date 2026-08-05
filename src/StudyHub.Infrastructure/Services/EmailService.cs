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
                    <div style="background-color: #ffffff; border-radius: 50%; width: 44px; height: 44px; text-align: center; box-shadow: 0 4px 12px rgba(91, 77, 255, 0.12); border: 1px solid #F1F3FE; display: inline-block; line-height: 44px;">
                        <img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAHgAAAB4CAYAAAA5ZDbSAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAA0BSURBVHhe7Z0/qCRJGcAvNDQ0NDQ0vNDQRDgwORDkQAQ1cQMFueQ2UJbD4AzERQRFBE8QEQ18ycFxiRuILGfgHYisILJq4JvpeW/f7r7XXfKr7m+m5ntd3dXd1dU18+YHX3B38/qm++vvq+9f1bzyyokTJ06cOLEM5+fmk+cb8zmvnJtP6785kRHGmE+gqFVx87X1pry/3lRnq035aL2pzGApysfronqf65yvb+5x3YsL8yn9/zwxIyh0tbr+/HpTPQhR5MWzylw+M52i/0bLalN+tN5UD8+L69fwCPo7nZiItdL1zRtYp374osSr58Y8f1GZ6+vK3JRmFGVpzPVNfZ3nLzqUX5SP8Rgn657Iel29iuWsN+W5VigKQJkpuGmU3qrwonp3tbl5XX/3Ex1grY1b3D5IHu6Ll2kU2sfL68o8u9pX9qoon9q1++TC27HR7vrmXv2g6oe2ucRSK+s6c6Sq6pfu4nJP0VfFRfXOyX03sL7W0e/ODfPAcrHWUFi7tVUXm+pnd1rRRKWuxeKGcX+HDAHevqLLFZ5J3/tRUxTmM25EjMViAccEy8qlo2ib0q2rV/WzODpwx6uifG5d2EW9xh4zRPrcp+O6H7As6edy8BBEuVaLGyNIuSuQ1q2KsrL3X5SP8WL6GR0suCZZa3mbU+WvucH6TGYga/NR5M8EGNu19ll1p6y2De5fRdsPD9Zlkw/KjeCiTuwgDdy67E11dnBKJge00WNRVoee+swFLnsbgBXl44OogtWFCwmmyuqurrehkE7Jukx5Nuv+tFWu7afWwdTYrs5dg3V5V+4sn2SpZNdyeSNzrR/nCkreFUbKJ9mVOGXNxXJPyh0PmUZ2a/IuWi6rk1ueBpa8y5UziK5Xxc13JBXKNaD64wfGvP2WMd99s/7nX/zEmLe+ZcwH7xnzMsP0zQ28GCjQzzwZDKXlqtz//seYX/3cmG982ZgvfWEn//j7/r/76uvG/PRHxvzrn/oKy4InxCPybBfpRhEESPkxp4aBWKurVFe+/fXb/04kN6umfmDTp6K8St6JknSIHu7S+Kx1rORk1VQAnfQpTdDVzB/biHnJ2nKftcaQHKx6G1lvqt9qXUTn/MJ8Vvq5SzTpY1trqCxp1QRd2/W4uH5N6yQq4poc4Z5pt+u2d8d8t8s3rSjVl7+lEu/Z/hGz2D+rn0t0v3hF+yT2M3/s9u+T1k312L5M6ZqnWyt/x99znTYuL4157w/dgVeXpLZqZ3pzQ+vmqX6TstZd+Z6b/o7rN+l/9/f2sS30b1N52Nq3a2z3CPl3qTzs8u/eNlb5pvm16l0s1+m+jXl1SflwX2Ddpf9d0lD2p/7f5eNdbepd21r+j7+veS32f6b8u0R+z/V1j0vL+z1uGpd37+38b3X59r3bHw+2/fP2c5k/n4upvGxs3/29ff/q8m7z0l9fP1c/d/3+/pT1ffW4dNzD+/tzx/s+bh231f2/Z125vevfv2+tue93vL7u+P3z9r0/13y71L9fWn6q06/r55q/m4+/V+vrqsc6f7fX5ed/2zXveb+e633d8b7255qve36u+/v6uv65n6+u69qf+7nu97ve11yv/Vzve/2+rr/7Xb72edXjmt+vdT2u6+fr/b123/dzvd/t61/v+rnv9zXv69r1Ndf/31y/r6vrz338d/l676n27+97fW3t/b7v2+t8fe1nXd93ve6+vu/3fe1r56t2uW99f93/9/V8bW7V91y/f1/ze1+X5/25znv/vrn+31c/9+fa/5trr++5Pv9/X9dzvf/9fd3Xff19j1/z537u4/3d1/e0nmt/+556vO6+1ufn/VzXdX/X+/fvuN73ff8b13d5/e7ve937e3352v93X/N7v29f17zvXy9fP1+veW1uXe4/3/e0vv7er9d8vO+/+5p/d9/X9z29r3+96rnf8/11/d1v+bn2df+6v25/rrn/+rnvb65vvv+u83v7e9+zXf5e71s+9rmvv7u/15v2e1zbfV2X+11fX5dve1zf87W+pvUvV//3eM1t/VrfU8+vj9vrft1zrfv62r++/u7j1/05X/fva/p+ve85ru37urau5r6m6/z7mte2v+e1+/nreZtrvW99Xfv/rq2t9zWf+5yva76urW//vu+p1+v9e/+/e3/f07W+vrv/7uu2/Prm+rvfdNznvt9731x/32u/r7W9vnV/znmtef+6p683+/b/vaf2c66t7fF93+trGtf83ff93e36e/1+t9e/N5vruN5zvea2vn/f+7mt53rfe3u+7/m5tvtzru+/v+v355r2ff0257y2++tr7mt/r7X/+rre13w9j+t///u69uf+uX+ur+//udf+XNfn2u/v5+u1vfe/P9fa2vv+9rN9vva5vv7ue2uf+/rv+/re05zv/XvveV2v//+25z3257q/P9fW3Ndzvv//2t9/v/V77+/5vu62vn/f63m9ru/+vv/f1+v8e++vu+e+53M+1/f3eM3/3dd//72n//84157v3/e6+ve0tn/Xv29f+5p/39fe0/X7uPaa9te+/+63Pff7mv8312vve+vntb/v+7qurffz+r6u/blvn/u59mt/r+/nvtf72u9//f3+9zX/7+Nce93T+1r3dV1b//71uf793te8z1//97H//3vvc62vtdd/+vva1+vrve5n2t5rnud2vN7v/f/f0/73ff3fa9/3df+7/2+tr//2uJ+v937+72v//77m59rnvu++tr6uue9ve+/xvd+2rvW/37a5x/ve+7//+rm2vv65n+u59r6ve0/f/+vte4zrc7+ue93Xv73Wfa3te/t73z9f9/f2/f/33ufafj62rn+/9rr//ffWvu/5ue97fe/n3z62fF3ze/+v57Xfe//3eO173+u537+/9r/f8/f4vt++1ufd//v//+rn+7r/d9/3fe/3t9f2veb+ve49vf5+x7r2e3+/tnff97+/r+/l672neW37mra55muea/+///p5/e/X/N5/d/v2sW9fe0z739e1/+9+39/fe/++7+u21uN//7Wvbfn21z43rd+05bne17/+3/u23udc3/972/u+v+45r//2eL9rfv5///3a95rmfd7/ft3b9/3v5//te/r//VzfV9vvvu99bf+8ve/5//t1bVvXfK3/+l//2vv8e+//fa1tXd++/bnuad/zve//ft2+/fne7/v/te+//fvev/v/vu75nvt/v2vf2/f6ue7vfe1rf3+/7+33fa///30tfW//vn2ub2t/zvf1v/321z4//79///a+9/l5fe3///e37//+7ev+//+/ve///3v///e///v///e///v///e///v///e/4E+189912E3/bQAAAABJRU5ErkJggg==" width="36" height="36" alt="🛡️" style="vertical-align: middle; display: inline-block;" />
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
                    <div style="background-color: #ffffff; border-radius: 50%; width: 44px; height: 44px; text-align: center; box-shadow: 0 4px 12px rgba(239, 68, 68, 0.12); border: 1px solid #FEE2E2; display: inline-block; line-height: 44px;">
                        <img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAHgAAAB4CAYAAAA5ZDbSAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAA1zSURBVHhe7Z09jBxFFscJCS+8kJDwQsILCBDRRugCgiPiEiQkkiOCBDkggAgnSIjIEbqAwHKCEyRLJCbCCdKoe2x2+VjveNf2fsxM1+lfU6+m+21XV3V3fc1s/6QXYHZ7p/vNe/W+qvqVVyYmJiYmJtIgZrO/ifn8n0aZzV7jvzOREWI2exWKWhXFf9ZF8WlVFHfXZfmgKkvRV9Zl+bAqy/u4zqooPlRfgL/zvzkRECh0WZZvVWV5y0mRv/8uxB9/dEo1n1//vabiH1VleXtZlgfwCPwzTYwESl0Vxb9hnfzhayU+fSqqxUJU5+dCXF6KQSyXQlxciOrZMyEWC6PyYenwGJN1j6Sazd6A5azL8uSaQkmZVcXV5B8onRTOv1xFcWdVlv/in32iA1ircov6QcqH+/x5HIVaqF6+FOKvv7hVH2Ltnly4ATwYBDZ4UPrB/fbbxnLgOnNkvRbV2Zmojo7qij6vyvKLyX0rsL7im99ww3hgsNZd4uJCiD//5C78mxutaESldYuVbhjub5dBgFdz3+uyXMAz8Xvfa8Rs9nojIj462ljAPoFlpRaUyZRuNnuDP4u9Q7njC3njjx+L6vSUP5q9ApF+9eRJ3W3fwrLEn8vOgyCqbrVwY2K14s9jP0Hkv1jAiitlzQ/hxfgz2lngmvRaC6tF/noTubwU1eHhdm3eh/wZAYZ2TyhQ3BSrNbFe8xz69s66bOSD2iUjn53QIH8ml42la+eUjBxQuanJzQ5Ob1m731dnBKJge00WNRVoee+swFLnsbgBXl44OogtWFCwmmyuqurrehkE7Jukx5Nuv+tFWu7afWwdTYrs5dg3V5V+4sn2SpZNdyeSNzrR/nCkreFUbKJ9mVOGXNxXJPyh0PmUZ2a/IuWi6rk1ueBpa8y5UziK5Xxc13JBXKNaD64wfGvP2WMd99s/7nX/zEmLe+ZcwH7xnzMsP0zQ28GCjQzzwZDKXlqtz//seYX/3cmG982ZgvfWEn//j7/r/76uvG/PRHxvzrn/oKy4InxCPybBfpRhEESPkxp4aBWKurVFe+/fXb/04kN6umfmDTp6K8St6JknSIHu7S+Kx1rORk1VQAnfQpTdDVzB/biHnJ2nKftcaQHKx6G1lvqt9qXUTn/MJ8Vvq5SzTpY1trqCxp1QRd2/W4uH5N6yQq4poc4Z5pt+u2d8d8t8s3rSjVl7+lEu/Z/hGz2D+rn0t0v3hF+yT2M3/s9u+T1k312L5M6ZqnWyt/x99znTYuL4157w/dgVeXpLZqZ3pzQ+vmqX6TstZd+Z6b/o7rN+l/9/f2sS30b1N52Nq3a2z3CPl3qTzs8u/eNlb5pvm16l0s1+m+jXl1SflwX2Ddpf9d0lD2p/7f5eNdbepd21r+j7+veS32f6b8u0R+z/V1j0vL+z1uGpd37+38b3X59r3bHw+2/fP2c5k/n4upvGxs3/29ff/q8m7z0l9fP1c/d/3+/pT1ffW4dNzD+/tzx/s+bh231f2/Z125vevfv2+tue93vL7u+P3z9r0/13y71L9fWn6q06/r55q/m4+/V+vrqsc6f7fX5ed/2zXveb+e633d8b7255qve36u+/v6uv65n6+u69qf+7nu97ve11yv/Vzve/2+rr/7Xb72edXjmt+vdT2u6+fr/b123/dzvd/t61/v+rnv9zXv69r1Ndf/31y/r6vrz338d/l676n27+97fW3t/b7v2+t8fe1nXd93ve6+vu/3fe1r56t2uW99f93/9/V8bW7V91y/f1/ze1+X5/25znv/vrn+31c/9+fa/5trr++5Pv9/X9dzvf/9fd3Xff19j1/z537u4/3d1/e0nmt/+556vO6+1ufn/VzXdX/X+/fvuN73ff8b13d5/e7ve937e3352v93X/N7v29f17zvXy9fP1+veW1uXe4/3/e0vv7er9d8vO+/+5p/d9/X9z29r3+96rnf8/11/d1v+bn2df+6v25/rrn/+rnvb65vvv+u83v7e9+zXf5e71s+9rmvv7u/15v2e1zbfV2X+11fX5dve1zf87W+pvUvV//3eM1t/VrfU8+vj9vrft1zrfv62r++/u7j1/05X/fva/p+ve85ru37urau5r6m6/z7mte2v+e1+/nreZtrvW99Xfv/rq2t9zWf+5yva76urW//vu+p1+v9e/+/e3/f07W+vrv/7uu2/Prm+rvfdNznvt9731x/32u/r7W9vnV/znmtef+6p683+/b/vaf2c66t7fF93+trGtf83ff93e36e/1+t9e/N5vruN5zvea2vn/f+7mt53rfe3u+7/m5tvtzru+/v+v355r2ff0257y2++tr7mt/r7X/+rre13w9j+t///u69uf+uX+ur+//udf+XNfn2u/v5+u1vfe/P9fa2vv+9rN9vva5vv7ue2uf+/rv+/re05zv/XvveV2v//+25z3257q/P9fW3Ndzvv//2t9/v/V77+/5vu62vn/f63m9ru/+vv/f1+v8e++vu+e+53M+1/f3eM3/3dd//72n//84157v3/e6+ve0tn/Xv29f+5p/39fe0/X7uPaa9te+/+63Pff7mv8312vve+vntb/v+7qurffz+r6u/blvn/u59mt/r+/nvtf72u9//f3+9zX/7+Nce93T+1r3dV1b//71uf793te8z1//97H//3vvc62vtdd/+vva1+vrve5n2t5rnud2vN7v/f/f0/73ff3fa9/3df+7/2+tr//2uJ+v937+72v//77m59rnvu++tr6uue9ve+/xvd+2rvW/37a5x/ve+7//+rm2vv65n+u59r6ve0/f/+vte4zrc7+ue93Xv73Wfa3te/t73z9f9/f2/f/33ufafj62rn+/9rr//ffWvu/5ue97fe/n3z62fF3ze/+v57Xfe//3eO173+u537+/9r/f8/f4vt++1ufd//v//+rn+7r/d9/3fe/3t9f2veb+ve49vf5+x7r2e3+/tnff97+/r+/l672neW37mra55muea/+///p5/e/X/N5/d/v2sW9fe0z739e1/+9+39/fe/++7+u21uN//7Wvbfn21z43rd+05bne17/+3/u23udc3/972/u+v+45r//2eL9rfv5///3a95rmfd7/ft3b9/3v5//te/r//VzfV9vvvu99bf+8ve/5//t1bVvXfK3/+l//2vv8e+//fa1tXd++/bnuad/zve//ft2+/fne7/v/te+//fvev/v/vu75nvt/v2vf2/f6ue7vfe1rf3+/7+33fa///30tfW//vn2ub2t/zvf1v/321z4//79///a+9/l5fe3///e37//+7ev+//+/ve///3v///e///v///e///v///e///v///e/4E+189912E3/bQAAAABJRU5ErkJggg==" width="36" height="36" alt="🛡️" style="vertical-align: middle; display: inline-block;" />
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
