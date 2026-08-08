using System;
using System.Collections.Generic;

namespace StudyHub.Application.DTOs.Admin
{
    public class SystemDashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveStudents { get; set; }
        public int BlockedUsers { get; set; }
        public int TotalTasks { get; set; }
        public int TotalStudyGroups { get; set; }
        public int ActiveStudyGroups { get; set; }
        public int InactiveStudyGroups { get; set; }
        public int NewStudyGroupsThisWeek { get; set; }
        public int TotalDocuments { get; set; }
        public List<MonthlyUserGrowthDto> UserGrowth { get; set; } = new List<MonthlyUserGrowthDto>();

        // Today system activities
        public int TasksCreatedToday { get; set; }
        public int PomodoroSessionsToday { get; set; }
        public int GroupMessagesToday { get; set; }
        public int GroupsCreatedToday { get; set; }

        // AI Assistant stats
        public int TotalAiUsage { get; set; }
        public int AiSummariesCount { get; set; }
        public int AiPlannerCount { get; set; }
        public int AiQnaCount { get; set; }

        // Recent 5 registered users
        public List<UserManagementDto> RecentUsers { get; set; } = new List<UserManagementDto>();
    }

    public class MonthlyUserGrowthDto
    {
        public string MonthLabel { get; set; } = null!;
        public int NewUsers { get; set; }
        public int TotalUsers { get; set; }
    }

    public class UserManagementDto
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; } = null!;
        public byte TrangThai { get; set; } // 1: Active, 0: Blocked
        public string? AnhDaiDien { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? LanDangNhapCuoi { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public byte TrangThai { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        public int MaVaiTro { get; set; }
    }

    public class GroupManagementDto
    {
        public int MaNhom { get; set; }
        public string TenNhom { get; set; } = null!;
        public string? MoTa { get; set; }
        public string? AnhDaiDien { get; set; }
        public string MaThamGia { get; set; } = null!;
        public int MaNguoiTao { get; set; }
        public string TenNguoiTao { get; set; } = null!;
        public string EmailNguoiTao { get; set; } = null!;
        public int? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public int SoLuongThanhVien { get; set; }
        public int SoLuongToiDa { get; set; }
        public byte TrangThai { get; set; } // 1: Active, 0: Locked/Dissolved
        public DateTime NgayTao { get; set; }
        public List<GroupMemberDto> ThanhVien { get; set; } = new List<GroupMemberDto>();
    }

    public class GroupMemberDto
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AnhDaiDien { get; set; }
        public string VaiTro { get; set; } = "Thành viên";
        public DateTime NgayThamGia { get; set; }
    }

    public class UpdateGroupStatusRequest
    {
        public byte TrangThai { get; set; }
    }
}
