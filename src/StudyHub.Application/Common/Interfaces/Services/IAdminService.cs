using System.Collections.Generic;
using System.Threading.Tasks;
using StudyHub.Application.DTOs.Admin;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IAdminService
    {
        Task<SystemDashboardStatsDto> GetSystemStatsAsync();
        Task<List<UserManagementDto>> GetUsersAsync(string? search = null, int? roleId = null, byte? status = null);
        Task<bool> ToggleUserStatusAsync(int userId, byte newStatus);
        Task<bool> UpdateUserRoleAsync(int userId, int newRoleId);
        Task<List<GroupManagementDto>> GetGroupsAsync(string? search = null, byte? status = null);
        Task<bool> ToggleGroupStatusAsync(int groupId, byte newStatus);
    }
}
