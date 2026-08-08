using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Admin;
using StudyHub.Domain.Entities;
using StudyHub.Persistence;

namespace StudyHub.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly StudyHubDbContext _dbContext;

        public AdminService(StudyHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SystemDashboardStatsDto> GetSystemStatsAsync()
        {
            var now = DateTime.Now;
            var todayLocal = now.Date;
            var todayUtc = DateTime.UtcNow.Date;
            var startOfWeek = todayLocal.AddDays(-(int)todayLocal.DayOfWeek + (int)DayOfWeek.Monday);

            var totalUsers = await _dbContext.NguoiDung.CountAsync(u => !u.DaXoa);
            var activeStudents = await _dbContext.NguoiDung.CountAsync(u => !u.DaXoa && u.TrangThai == 1 && u.MaVaiTro != 1);
            var blockedUsers = await _dbContext.NguoiDung.CountAsync(u => !u.DaXoa && u.TrangThai == 0);
            var totalTasks = await _dbContext.CongViec.CountAsync(t => !t.DaXoa);

            // Study Groups breakdown
            var totalGroups = await _dbContext.NhomHocTap.CountAsync(g => !g.DaXoa);
            var activeGroups = await _dbContext.NhomHocTap.CountAsync(g => !g.DaXoa && g.TrangThai == 1);
            var inactiveGroups = await _dbContext.NhomHocTap.CountAsync(g => !g.DaXoa && g.TrangThai == 0);
            var newGroupsThisWeek = await _dbContext.NhomHocTap.CountAsync(g => !g.DaXoa && (g.NgayTao >= startOfWeek || g.NgayTao >= startOfWeek.ToUniversalTime()));

            var totalDocs = await _dbContext.TaiLieu.CountAsync(d => !d.DaXoa);

            // Today System Activities
            var tasksCreatedToday = await _dbContext.CongViec.CountAsync(t => !t.DaXoa && (t.NgayTao >= todayUtc || t.NgayTao >= todayLocal));
            var pomodoroToday = await _dbContext.PomodoroSession.CountAsync(p => p.ThoiGianBatDau >= todayUtc || p.ThoiGianBatDau >= todayLocal);
            var messagesToday = await _dbContext.TinNhan.CountAsync(m => !m.DaXoa && (m.NgayGui >= todayUtc || m.NgayGui >= todayLocal));
            var groupsCreatedToday = await _dbContext.NhomHocTap.CountAsync(g => !g.DaXoa && (g.NgayTao >= todayUtc || g.NgayTao >= todayLocal));

            // AI Assistant stats
            var aiMessagesCount = await _dbContext.TinNhanAI.CountAsync();
            var aiSummariesCount = await _dbContext.LichSuTomTat.CountAsync();
            var aiPlannerCount = await _dbContext.CongViec.CountAsync(t => !t.DaXoa);
            var totalAiUsage = aiMessagesCount + aiSummariesCount;

            // Calculate User Growth for last 6 months
            var monthlyUserGrowth = new List<MonthlyUserGrowthDto>();
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

                var newInMonth = await _dbContext.NguoiDung.CountAsync(u => !u.DaXoa && u.NgayTao >= startOfMonth && u.NgayTao <= endOfMonth);
                var totalUntilMonth = await _dbContext.NguoiDung.CountAsync(u => !u.DaXoa && u.NgayTao <= endOfMonth);

                monthlyUserGrowth.Add(new MonthlyUserGrowthDto
                {
                    MonthLabel = $"Tháng {startOfMonth.Month}",
                    NewUsers = newInMonth,
                    TotalUsers = totalUntilMonth
                });
            }

            // Recent 5 Registered Users
            var recentUsersList = await _dbContext.NguoiDung
                .Include(u => u.VaiTro)
                .Where(u => !u.DaXoa)
                .OrderByDescending(u => u.NgayTao)
                .Take(5)
                .Select(u => new UserManagementDto
                {
                    MaNguoiDung = u.MaNguoiDung,
                    HoTen = u.HoTen,
                    Email = u.Email,
                    SoDienThoai = u.SoDienThoai,
                    MaVaiTro = u.MaVaiTro,
                    TenVaiTro = u.VaiTro != null ? u.VaiTro.TenVaiTro : (u.MaVaiTro == 1 ? "System Admin" : "Sinh viên"),
                    TrangThai = u.TrangThai,
                    AnhDaiDien = u.AnhDaiDien,
                    NgayTao = u.NgayTao,
                    LanDangNhapCuoi = u.LanDangNhapCuoi
                })
                .ToListAsync();

            return new SystemDashboardStatsDto
            {
                TotalUsers = totalUsers,
                ActiveStudents = activeStudents,
                BlockedUsers = blockedUsers,
                TotalTasks = totalTasks,
                TotalStudyGroups = totalGroups,
                ActiveStudyGroups = activeGroups,
                InactiveStudyGroups = inactiveGroups,
                NewStudyGroupsThisWeek = newGroupsThisWeek,
                TotalDocuments = totalDocs,
                UserGrowth = monthlyUserGrowth,

                TasksCreatedToday = tasksCreatedToday,
                PomodoroSessionsToday = pomodoroToday,
                GroupMessagesToday = messagesToday,
                GroupsCreatedToday = groupsCreatedToday,

                TotalAiUsage = totalAiUsage,
                AiSummariesCount = aiSummariesCount,
                AiPlannerCount = aiPlannerCount,
                AiQnaCount = aiMessagesCount,

                RecentUsers = recentUsersList
            };
        }

        public async Task<List<UserManagementDto>> GetUsersAsync(string? search = null, int? roleId = null, byte? status = null)
        {
            var query = _dbContext.NguoiDung
                .Include(u => u.VaiTro)
                .AsNoTracking()
                .Where(u => !u.DaXoa);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u => u.HoTen.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
            }

            if (roleId.HasValue && roleId.Value > 0)
            {
                query = query.Where(u => u.MaVaiTro == roleId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(u => u.TrangThai == status.Value);
            }

            var users = await query
                .OrderByDescending(u => u.NgayTao)
                .Select(u => new UserManagementDto
                {
                    MaNguoiDung = u.MaNguoiDung,
                    HoTen = u.HoTen,
                    Email = u.Email,
                    SoDienThoai = u.SoDienThoai,
                    MaVaiTro = u.MaVaiTro,
                    TenVaiTro = u.VaiTro != null ? u.VaiTro.TenVaiTro : (u.MaVaiTro == 1 ? "System Admin" : "Sinh viên"),
                    TrangThai = u.TrangThai,
                    AnhDaiDien = u.AnhDaiDien,
                    NgayTao = u.NgayTao,
                    LanDangNhapCuoi = u.LanDangNhapCuoi
                })
                .ToListAsync();

            return users;
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, byte newStatus)
        {
            var user = await _dbContext.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == userId && !u.DaXoa);
            if (user == null) return false;

            user.TrangThai = newStatus;
            user.NgayCapNhat = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, int newRoleId)
        {
            var user = await _dbContext.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == userId && !u.DaXoa);
            if (user == null) return false;

            user.MaVaiTro = newRoleId;
            user.NgayCapNhat = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<GroupManagementDto>> GetGroupsAsync(string? search = null, byte? status = null)
        {
            var query = _dbContext.NhomHocTap
                .Include(g => g.MonHoc)
                .Include(g => g.ThanhVienNhom)
                    .ThenInclude(tv => tv.NguoiDung)
                .Where(g => !g.DaXoa)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(g => g.TrangThai == status.Value);
            }

            var groups = await query.OrderByDescending(g => g.NgayTao).ToListAsync();
            var allUsers = await _dbContext.NguoiDung.AsNoTracking().ToDictionaryAsync(u => u.MaNguoiDung, u => u);

            var result = groups.Select(g => {
                var creator = allUsers.ContainsKey(g.MaNguoiTao) ? allUsers[g.MaNguoiTao] : null;
                return new GroupManagementDto
                {
                    MaNhom = g.MaNhom,
                    TenNhom = g.TenNhom ?? "Nhóm học tập",
                    MoTa = g.MoTa,
                    AnhDaiDien = g.AnhDaiDien,
                    MaThamGia = g.MaThamGia ?? "---",
                    MaNguoiTao = g.MaNguoiTao,
                    TenNguoiTao = creator != null ? creator.HoTen : "Thành viên",
                    EmailNguoiTao = creator != null ? creator.Email : "---",
                    MaMonHoc = g.MaMonHoc,
                    TenMonHoc = g.MonHoc != null ? g.MonHoc.TenMonHoc : null,
                    SoLuongThanhVien = g.ThanhVienNhom != null ? g.ThanhVienNhom.Count : 0,
                    SoLuongToiDa = g.SoLuongToiDa > 0 ? g.SoLuongToiDa : 10,
                    TrangThai = g.TrangThai,
                    NgayTao = g.NgayTao,
                    ThanhVien = g.ThanhVienNhom != null ? g.ThanhVienNhom.Select(tv => new GroupMemberDto
                    {
                        MaNguoiDung = tv.MaNguoiDung,
                        HoTen = tv.NguoiDung != null ? tv.NguoiDung.HoTen : (allUsers.ContainsKey(tv.MaNguoiDung) ? allUsers[tv.MaNguoiDung].HoTen : "Thành viên"),
                        Email = tv.NguoiDung != null ? tv.NguoiDung.Email : (allUsers.ContainsKey(tv.MaNguoiDung) ? allUsers[tv.MaNguoiDung].Email : "---"),
                        AnhDaiDien = tv.NguoiDung?.AnhDaiDien ?? (allUsers.ContainsKey(tv.MaNguoiDung) ? allUsers[tv.MaNguoiDung].AnhDaiDien : null),
                        VaiTro = tv.VaiTro == 2 ? "Trưởng nhóm" : (tv.VaiTro == 1 ? "Quản trị viên" : "Thành viên"),
                        NgayThamGia = tv.NgayThamGia
                    }).ToList() : new List<GroupMemberDto>()
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                result = result.Where(g =>
                    g.TenNhom.ToLower().Contains(s) ||
                    (g.MoTa != null && g.MoTa.ToLower().Contains(s)) ||
                    g.TenNguoiTao.ToLower().Contains(s) ||
                    g.MaThamGia.ToLower().Contains(s)
                ).ToList();
            }

            return result;
        }

        public async Task<bool> ToggleGroupStatusAsync(int groupId, byte newStatus)
        {
            var group = await _dbContext.NhomHocTap.FirstOrDefaultAsync(g => g.MaNhom == groupId && !g.DaXoa);
            if (group == null) return false;

            group.TrangThai = newStatus;
            group.NgayCapNhat = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
