using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Admin;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    [Route("api/v1/admin")]
    public class AdminController : ApiControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ICurrentUserService _currentUserService;

        public AdminController(
            IAdminService adminService,
            ICurrentUserService currentUserService)
        {
            _adminService = adminService;
            _currentUserService = currentUserService;
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

        [HttpGet("stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            GetCurrentUserId();
            var stats = await _adminService.GetSystemStatsAsync();
            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int? roleId, [FromQuery] byte? status)
        {
            GetCurrentUserId();
            var users = await _adminService.GetUsersAsync(search, roleId, status);
            return Ok(users);
        }

        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> ToggleUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            GetCurrentUserId();
            var result = await _adminService.ToggleUserStatusAsync(id, request.TrangThai);
            if (!result) return NotFound(new { message = "Không tìm thấy người dùng." });
            return Ok(new { message = "Cập nhật trạng thái người dùng thành công." });
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
        {
            GetCurrentUserId();
            var result = await _adminService.UpdateUserRoleAsync(id, request.MaVaiTro);
            if (!result) return NotFound(new { message = "Không tìm thấy người dùng." });
            return Ok(new { message = "Cập nhật vai trò người dùng thành công." });
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups([FromQuery] string? search, [FromQuery] byte? status)
        {
            GetCurrentUserId();
            var groups = await _adminService.GetGroupsAsync(search, status);
            return Ok(groups);
        }

        [HttpPut("groups/{id}/status")]
        public async Task<IActionResult> ToggleGroupStatus(int id, [FromBody] UpdateGroupStatusRequest request)
        {
            GetCurrentUserId();
            var result = await _adminService.ToggleGroupStatusAsync(id, request.TrangThai);
            if (!result) return NotFound(new { message = "Không tìm thấy nhóm học tập." });
            return Ok(new { message = "Cập nhật trạng thái nhóm học tập thành công." });
        }
    }
}
