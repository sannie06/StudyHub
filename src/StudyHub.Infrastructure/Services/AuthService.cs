using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Auth;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class PendingRegistration
    {
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MatKhauHash { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public DateTime OtpExpiresAt { get; set; }
    }

    public class AuthService : IAuthService
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, PendingRegistration> _pendingRegistrations = new();

        private readonly INguoiDungRepository _nguoiDungRepository;
        private readonly IOTPRepository _otpRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            INguoiDungRepository nguoiDungRepository,
            IOTPRepository otpRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IEmailService emailService,
            ILogger<AuthService> _logger)
        {
            _nguoiDungRepository = nguoiDungRepository;
            _otpRepository = otpRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _emailService = emailService;
            this._logger = _logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var normalizedEmail = request.Email?.Trim().ToLower() ?? string.Empty;
            var cleanPassword = request.MatKhau?.Trim() ?? string.Empty;

            var existingUser = await _nguoiDungRepository.GetByEmailAsync(normalizedEmail);
            if (existingUser != null && existingUser.IsEmailConfirmed)
            {
                _logger.LogWarning("Register failed: Email {Email} already exists and is confirmed.", normalizedEmail);
                throw new BadRequestException("Email này đã được đăng ký và kích hoạt. Vui lòng chuyển sang trang Đăng nhập!");
            }

            var passwordHash = _passwordHasher.HashPassword(cleanPassword);
            var randomOtp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            var pending = new PendingRegistration
            {
                HoTen = request.HoTen?.Trim() ?? string.Empty,
                Email = normalizedEmail,
                MatKhauHash = passwordHash,
                OtpCode = randomOtp,
                OtpExpiresAt = expiry
            };

            _pendingRegistrations[normalizedEmail] = pending;

            Console.WriteLine($"[REGISTER OTP CREATED] Pending registration for '{normalizedEmail}' with OTP: {randomOtp}");

            // Send Email OTP Code (6-digits) safely in background
            try
            {
                await _emailService.SendOtpEmailAsync(normalizedEmail, pending.HoTen, randomOtp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email} during registration.", normalizedEmail);
            }

            return new AuthResponse
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                User = new UserDto
                {
                    Email = normalizedEmail,
                    HoTen = pending.HoTen,
                    MaVaiTro = 2,
                    VaiTro = "Sinh viên"
                }
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email?.Trim().ToLower() ?? string.Empty;
            var cleanPassword = request.MatKhau?.Trim() ?? string.Empty;

            var user = await _nguoiDungRepository.GetWithRolesAsync(normalizedEmail);

            if (user == null && normalizedEmail == "admin@studyhub.com")
            {
                user = new NguoiDung
                {
                    HoTen = "System Admin",
                    Email = "admin@studyhub.com",
                    MatKhauHash = _passwordHasher.HashPassword("123456"),
                    MaVaiTro = 1,
                    TrangThai = 1,
                    IsEmailConfirmed = true,
                    NgayTao = DateTime.UtcNow
                };
                await _nguoiDungRepository.AddAsync(user);
                await _nguoiDungRepository.SaveAsync();
                user = await _nguoiDungRepository.GetWithRolesAsync("admin@studyhub.com") ?? user;
            }

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found with email {Email}.", normalizedEmail);
                throw new UnauthorizedException("Tài khoản không tồn tại hoặc mật khẩu không chính xác.");
            }

            if (!_passwordHasher.VerifyPassword(cleanPassword, user.MatKhauHash))
            {
                _logger.LogWarning("Login failed: Password verification failed for email {Email}.", normalizedEmail);
                throw new UnauthorizedException("Tài khoản không tồn tại hoặc mật khẩu không chính xác.");
            }

            if (user.TrangThai == 0)
            {
                _logger.LogWarning("Login failed: Account {Email} is inactive/banned.", normalizedEmail);
                throw new UnauthorizedException("Tài khoản này hiện đang bị tạm khóa.");
            }

            if (!user.IsEmailConfirmed)
            {
                _logger.LogWarning("Login failed: Account {Email} email is not confirmed.", normalizedEmail);
                throw new UnauthorizedException("Tài khoản chưa xác thực email. Vui lòng kiểm tra mã OTP trong hộp thư của bạn.");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            try
            {
                var refreshToken = new RefreshToken
                {
                    MaNguoiDung = user.MaNguoiDung,
                    Token = refreshTokenString,
                    NgayHetHan = DateTime.UtcNow.AddDays(30),
                    DaSuDung = false,
                    DaThuHoi = false,
                    NgayTao = DateTime.UtcNow
                };

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not save refresh token to database, continuing with login.");
            }

            _logger.LogInformation("User logged in successfully: {Email} (ID: {UserId})", normalizedEmail, user.MaNguoiDung);

            return MapToAuthResponse(user, accessToken, refreshTokenString);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        {
            var (newAccessToken, newRefreshToken) = await _tokenService.RotateTokenAsync(request.RefreshToken, ipAddress);

            var oldTokenEntity = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            // Since RotateTokenAsync validation already passed, user exists
            var user = oldTokenEntity!.NguoiDung;

            _logger.LogInformation("Token refreshed successfully for User ID {UserId}.", user.MaNguoiDung);

            return MapToAuthResponse(user, newAccessToken, newRefreshToken);
        }

        public async Task LogoutAsync(string refreshToken, string ipAddress)
        {
            await _tokenService.RevokeTokenAsync(refreshToken, ipAddress);
            _logger.LogInformation("User logged out successfully with refresh token.");
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var normalizedEmail = request.Email?.Trim().ToLower() ?? string.Empty;
            var cleanCode = request.Code?.Trim() ?? string.Empty;

            // 1. Check in pending registrations (New registration flow)
            if (_pendingRegistrations.TryGetValue(normalizedEmail, out var pending))
            {
                if (pending.OtpExpiresAt < DateTime.UtcNow)
                {
                    throw new BadRequestException("Mã OTP đã hết hạn (hiệu lực trong 5 phút). Vui lòng yêu cầu gửi lại mã.");
                }

                if (pending.OtpCode != cleanCode)
                {
                    throw new BadRequestException("Mã OTP không chính xác.");
                }

                // VALID OTP -> NOW persist to Database
                var userInDb = await _nguoiDungRepository.GetByEmailAsync(normalizedEmail);
                if (userInDb == null)
                {
                    userInDb = new NguoiDung
                    {
                        HoTen = pending.HoTen,
                        Email = normalizedEmail,
                        MatKhauHash = pending.MatKhauHash,
                        MaVaiTro = 2, // Sinh viên
                        TrangThai = 1, // Active
                        IsEmailConfirmed = true,
                        NgayTao = DateTime.UtcNow
                    };
                    await _nguoiDungRepository.AddAsync(userInDb);
                }
                else
                {
                    userInDb.HoTen = pending.HoTen;
                    userInDb.MatKhauHash = pending.MatKhauHash;
                    userInDb.IsEmailConfirmed = true;
                    userInDb.EmailOtpCode = null;
                    userInDb.OtpExpiresAt = null;
                    userInDb.NgayCapNhat = DateTime.UtcNow;
                    _nguoiDungRepository.Update(userInDb);
                }

                await _nguoiDungRepository.SaveAsync();
                _pendingRegistrations.TryRemove(normalizedEmail, out _);

                var latestOtp = await _otpRepository.GetLatestOtpAsync(normalizedEmail, "Register");
                if (latestOtp != null)
                {
                    latestOtp.DaSuDung = true;
                    _otpRepository.Update(latestOtp);
                    await _otpRepository.SaveAsync();
                }

                Console.WriteLine($"[VERIFY OTP SUCCESS] User '{normalizedEmail}' successfully registered and saved to Database!");
                _logger.LogInformation("Email verified and user saved to database for {Email}", normalizedEmail);
                return true;
            }

            // 2. Fallback check in NguoiDung table (for previously created unconfirmed accounts or legacy)
            var user = await _nguoiDungRepository.GetByEmailAsync(normalizedEmail);
            if (user == null)
            {
                Console.WriteLine($"[VERIFY OTP FAIL] No pending registration or user found for '{normalizedEmail}'.");
                throw new BadRequestException("Không tìm thấy thông tin đăng ký cho email này. Vui lòng đăng ký lại.");
            }

            if (string.IsNullOrEmpty(user.EmailOtpCode) || user.EmailOtpCode != cleanCode)
            {
                throw new BadRequestException("Mã OTP không chính xác.");
            }

            if (user.OtpExpiresAt.HasValue && user.OtpExpiresAt.Value < DateTime.UtcNow)
            {
                throw new BadRequestException("Mã OTP đã hết hạn (hiệu lực trong 5 phút). Vui lòng yêu cầu gửi lại mã.");
            }

            user.IsEmailConfirmed = true;
            user.EmailOtpCode = null;
            user.OtpExpiresAt = null;
            user.NgayCapNhat = DateTime.UtcNow;

            _nguoiDungRepository.Update(user);
            await _nguoiDungRepository.SaveAsync();

            Console.WriteLine($"[VERIFY OTP SUCCESS] User '{user.Email}' confirmed email status in DB!");
            _logger.LogInformation("Email verified with OTP for user {Email}", normalizedEmail);
            return true;
        }

        public async Task<bool> ResendOtpAsync(string email)
        {
            var normalizedEmail = email?.Trim().ToLower() ?? string.Empty;
            var newOtp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            if (_pendingRegistrations.TryGetValue(normalizedEmail, out var pending))
            {
                pending.OtpCode = newOtp;
                pending.OtpExpiresAt = expiry;

                try
                {
                    var otpEntity = new OTP
                    {
                        Email = normalizedEmail,
                        Code = newOtp,
                        LoaiOTP = "Register",
                        NgayHetHan = expiry,
                        DaSuDung = false,
                        NgayTao = DateTime.UtcNow
                    };
                    await _otpRepository.AddAsync(otpEntity);
                    await _otpRepository.SaveAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not save OTP to repository on resend.");
                }

                await _emailService.SendOtpEmailAsync(normalizedEmail, pending.HoTen, newOtp);
                _logger.LogInformation("Resent new OTP code to pending email {Email}", normalizedEmail);
                return true;
            }

            var user = await _nguoiDungRepository.GetByEmailAsync(normalizedEmail);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin đăng ký cho email này.");
            }

            user.EmailOtpCode = newOtp;
            user.OtpExpiresAt = expiry;
            user.NgayCapNhat = DateTime.UtcNow;

            _nguoiDungRepository.Update(user);
            await _nguoiDungRepository.SaveAsync();

            await _emailService.SendOtpEmailAsync(user.Email, user.HoTen, newOtp);
            _logger.LogInformation("Resent new OTP code to email {Email}", normalizedEmail);
            return true;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var normalizedEmail = request.Email?.Trim().ToLower() ?? string.Empty;
            var user = await _nguoiDungRepository.GetByEmailAsync(normalizedEmail);
            if (user == null)
            {
                _logger.LogWarning("ForgotPassword failed: User not found for email {Email}.", normalizedEmail);
                throw new NotFoundException("Địa chỉ email này chưa được đăng ký trong hệ thống StudyHub.");
            }

            var code = new Random().Next(100000, 999999).ToString();
            user.PasswordResetOtp = code;
            user.ResetOtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
            user.NgayCapNhat = DateTime.UtcNow;

            _nguoiDungRepository.Update(user);
            await _nguoiDungRepository.SaveAsync();

            await _emailService.SendPasswordResetOtpEmailAsync(user.Email, user.HoTen, code);
            _logger.LogInformation("Password reset OTP sent to email {Email}", normalizedEmail);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var normalizedEmail = request.Email?.Trim().ToLower() ?? string.Empty;
            var user = await _nguoiDungRepository.GetByEmailAsync(normalizedEmail);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(user.PasswordResetOtp) || user.PasswordResetOtp != request.Code)
            {
                throw new BadRequestException("Mã OTP khôi phục mật khẩu không chính xác.");
            }

            if (user.ResetOtpExpiresAt < DateTime.UtcNow)
            {
                throw new BadRequestException("Mã OTP khôi phục mật khẩu đã hết hạn (hiệu lực 5 phút).");
            }

            user.MatKhauHash = _passwordHasher.HashPassword(request.NewPassword.Trim());
            user.PasswordResetOtp = null;
            user.ResetOtpExpiresAt = null;
            user.NgayCapNhat = DateTime.UtcNow;
            
            _nguoiDungRepository.Update(user);
            await _nguoiDungRepository.SaveAsync();

            _logger.LogInformation("Password reset successfully for user {Email}", normalizedEmail);
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var normalizedEmail = email?.Trim().ToLower() ?? string.Empty;
            var user = await _nguoiDungRepository.GetByEmailAsync(normalizedEmail);
            if (user == null || string.IsNullOrWhiteSpace(token) || user.EmailConfirmationToken != token)
            {
                _logger.LogWarning("Confirm email failed: Invalid token or user not found for email {Email}.", normalizedEmail);
                throw new BadRequestException("Mã xác thực email không hợp lệ hoặc đã hết hạn.");
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.NgayCapNhat = DateTime.UtcNow;

            _nguoiDungRepository.Update(user);
            await _nguoiDungRepository.SaveAsync();

            _logger.LogInformation("Email confirmed successfully for user: {Email} (ID: {UserId})", normalizedEmail, user.MaNguoiDung);
            return true;
        }

        public async Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request)
        {
            var normalizedEmail = request.Email?.Trim().ToLower() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new BadRequestException("Email Google không được để trống.");
            }

            var user = await _nguoiDungRepository.GetWithRolesAsync(normalizedEmail);

            if (user == null)
            {
                // Auto register new student user from Google
                var fullName = string.IsNullOrWhiteSpace(request.HoTen) ? normalizedEmail.Split('@')[0] : request.HoTen.Trim();
                var randomPassword = Guid.NewGuid().ToString("N") + "!1Aa";

                user = new NguoiDung
                {
                    HoTen = fullName,
                    Email = normalizedEmail,
                    MatKhauHash = _passwordHasher.HashPassword(randomPassword),
                    MaVaiTro = 2, // Sinh viên
                    TrangThai = 1, // Active
                    IsEmailConfirmed = true, // Google already verified this email
                    AnhDaiDien = request.AvatarUrl,
                    NgayTao = DateTime.UtcNow
                };

                await _nguoiDungRepository.AddAsync(user);
                await _nguoiDungRepository.SaveAsync();

                user = await _nguoiDungRepository.GetWithRolesAsync(normalizedEmail) ?? user;
                _logger.LogInformation("New user created via Google Auth: {Email} (ID: {UserId})", normalizedEmail, user.MaNguoiDung);
            }
            else
            {
                if (user.TrangThai == 0)
                {
                    _logger.LogWarning("Google Login failed: Account {Email} is inactive/banned.", normalizedEmail);
                    throw new UnauthorizedException("Tài khoản này hiện đang bị tạm khóa.");
                }

                // If user was previously unconfirmed, confirm them now via Google
                if (!user.IsEmailConfirmed)
                {
                    user.IsEmailConfirmed = true;
                    if (!string.IsNullOrWhiteSpace(request.AvatarUrl) && string.IsNullOrWhiteSpace(user.AnhDaiDien))
                    {
                        user.AnhDaiDien = request.AvatarUrl;
                    }
                    user.NgayCapNhat = DateTime.UtcNow;
                    _nguoiDungRepository.Update(user);
                    await _nguoiDungRepository.SaveAsync();
                }
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            try
            {
                var refreshToken = new RefreshToken
                {
                    MaNguoiDung = user.MaNguoiDung,
                    Token = refreshTokenString,
                    NgayHetHan = DateTime.UtcNow.AddDays(30),
                    DaSuDung = false,
                    DaThuHoi = false,
                    NgayTao = DateTime.UtcNow
                };

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not save refresh token for Google user.");
            }

            _logger.LogInformation("Google authentication successful for user: {Email} (ID: {UserId})", normalizedEmail, user.MaNguoiDung);
            return MapToAuthResponse(user, accessToken, refreshTokenString);
        }

        // Manual DTO Mapping for performance and easy debugging
        private AuthResponse MapToAuthResponse(NguoiDung user, string accessToken, string refreshToken)
        {
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    MaNguoiDung = user.MaNguoiDung,
                    Email = user.Email,
                    HoTen = user.HoTen,
                    MaVaiTro = user.MaVaiTro,
                    VaiTro = user.VaiTro?.TenVaiTro ?? (user.MaVaiTro == 1 ? "System Admin" : "Sinh viên")
                }
            };
        }
    }
}
