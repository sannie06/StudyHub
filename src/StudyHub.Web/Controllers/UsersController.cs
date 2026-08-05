using System;
using System.IO;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using ValidationException = StudyHub.Application.Common.Exceptions.ValidationException;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.User;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
        private readonly IWebHostEnvironment _environment;

        public UsersController(
            IUserService userService,
            ICurrentUserService currentUserService,
            IValidator<UpdateProfileRequest> updateProfileValidator,
            IValidator<ChangePasswordRequest> changePasswordValidator,
            IWebHostEnvironment environment)
        {
            _userService = userService;
            _currentUserService = currentUserService;
            _updateProfileValidator = updateProfileValidator;
            _changePasswordValidator = changePasswordValidator;
            _environment = environment;
        }

        private int GetCurrentUserId()
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                throw new UnauthorizedException("Người dùng chưa được xác thực.");
            }
            return userId.Value;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _updateProfileValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var updatedProfile = await _userService.UpdateProfileAsync(userId, request);
            return Ok(updatedProfile);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _changePasswordValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _userService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Tập tin tải lên không hợp lệ.");
            }

            // Verify file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (Array.IndexOf(allowedExtensions, extension) < 0)
            {
                throw new BadRequestException("Định dạng ảnh không được hỗ trợ (chỉ chấp nhận JPG, PNG, WEBP).");
            }

            // Create uploads folder inside wwwroot if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"avatar_{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"/uploads/avatars/{fileName}";
            var updatedUrl = await _userService.UpdateAvatarAsync(userId, avatarUrl);

            return Ok(new { avatarUrl = updatedUrl });
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var userId = GetCurrentUserId();
            var stats = await _userService.GetStatisticsAsync(userId);
            return Ok(stats);
        }
    }
}
