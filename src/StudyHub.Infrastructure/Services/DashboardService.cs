using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Dashboard;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IGenericRepository<NguoiDung> _userRepository;
        private readonly IGenericRepository<MonHoc> _subjectRepository;
        private readonly IGenericRepository<CongViec> _taskRepository;
        private readonly ILichHocRepository _classScheduleRepository;
        private readonly ILichThiRepository _examScheduleRepository;
        private readonly INhomHocTapRepository _groupRepository;
        private readonly IThanhVienNhomRepository _groupMemberRepository;
        private readonly ITaiLieuRepository _documentRepository;
        private readonly IThongBaoRepository _notificationRepository;

        public DashboardService(
            IGenericRepository<NguoiDung> userRepository,
            IGenericRepository<MonHoc> subjectRepository,
            IGenericRepository<CongViec> taskRepository,
            ILichHocRepository classScheduleRepository,
            ILichThiRepository examScheduleRepository,
            INhomHocTapRepository groupRepository,
            IThanhVienNhomRepository groupMemberRepository,
            ITaiLieuRepository documentRepository,
            IThongBaoRepository notificationRepository)
        {
            _userRepository = userRepository;
            _subjectRepository = subjectRepository;
            _taskRepository = taskRepository;
            _classScheduleRepository = classScheduleRepository;
            _examScheduleRepository = examScheduleRepository;
            _groupRepository = groupRepository;
            _groupMemberRepository = groupMemberRepository;
            _documentRepository = documentRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // 1. User Info
            var user = await _userRepository.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.MaNguoiDung == userId);

            var userProfile = new DashboardUserProfileDto
            {
                MaNguoiDung = userId,
                HoTen = user?.HoTen ?? "Người dùng",
                Email = user?.Email ?? string.Empty,
                Avatar = user?.AnhDaiDien,
                VaiTro = "Học sinh"
            };

            // 2. Statistics & Weekly Progress
            var totalSubjects = await _subjectRepository.GetQueryable().AsNoTracking().CountAsync(s => s.TrangThai == 1);
            var userTasks = await _taskRepository.GetQueryable().AsNoTracking().Where(t => t.MaNguoiDung == userId && !t.DaXoa).ToListAsync();

            var totalTasks = userTasks.Count;
            var completedTasks = userTasks.Count(t => t.TrangThai == 3);
            var pendingTasks = userTasks.Count(t => t.TrangThai != 3);
            var deadlinesNext3Days = userTasks.Count(t => t.TrangThai != 3 && t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date >= today && t.HanHoanThanh.Value.Date <= today.AddDays(3));

            var unreadNotificationsCount = await _notificationRepository.GetQueryable()
                .AsNoTracking()
                .CountAsync(n => n.MaNguoiDung == userId && !n.DaDoc && !n.DaXoa);

            var statistics = new DashboardStatisticsDto
            {
                TongSoMonHoc = totalSubjects,
                TongSoCongViec = totalTasks,
                CongViecHoanThanh = completedTasks,
                CongViecChuaHoanThanh = pendingTasks,
                DeadlineHomNay = deadlinesNext3Days,
                ThongBaoChuaDoc = unreadNotificationsCount
            };

            // Calculate last 7 days progress
            var weeklyProgress = new List<WeeklyProgressDto>();
            for (int i = 6; i >= 0; i--)
            {
                var targetDate = today.AddDays(-i);
                var dayName = targetDate.ToString("ddd");
                var completedOnDay = userTasks.Count(t => t.TrangThai == 3 && t.NgayHoanThanh.HasValue && t.NgayHoanThanh.Value.Date == targetDate);
                var createdOnDay = userTasks.Count(t => t.NgayTao.Date == targetDate);

                weeklyProgress.Add(new WeeklyProgressDto
                {
                    DayName = dayName,
                    CompletedCount = completedOnDay,
                    CreatedCount = createdOnDay
                });
            }

            // 3. Today's Tasks
            var todayTasks = userTasks
                .Where(t => t.TrangThai != 3 && 
                           ((t.NgayBatDau.HasValue && t.NgayBatDau.Value.Date <= today) || 
                            (t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date >= today)))
                .Take(5)
                .Select(t => new DashboardTaskItemDto
                {
                    MaCongViec = t.MaCongViec,
                    TieuDe = t.TieuDe,
                    DoUuTien = t.DoUuTien,
                    TrangThai = t.TrangThai,
                    TiLeHoanThanh = t.TiLeHoanThanh,
                    HanHoanThanh = t.HanHoanThanh,
                    TenMonHoc = t.MonHoc?.TenMonHoc
                })
                .ToList();

            // 4. Upcoming Deadlines
            var upcomingDeadlines = userTasks
                .Where(t => t.TrangThai != 3 && t.HanHoanThanh.HasValue && t.HanHoanThanh.Value >= DateTime.UtcNow)
                .OrderBy(t => t.HanHoanThanh)
                .Take(5)
                .Select(t => new DashboardTaskItemDto
                {
                    MaCongViec = t.MaCongViec,
                    TieuDe = t.TieuDe,
                    DoUuTien = t.DoUuTien,
                    TrangThai = t.TrangThai,
                    TiLeHoanThanh = t.TiLeHoanThanh,
                    HanHoanThanh = t.HanHoanThanh,
                    TenMonHoc = t.MonHoc?.TenMonHoc
                })
                .ToList();

            // 5. Today's Class Schedule
            var todayClasses = await _classScheduleRepository.GetQueryable()
                .AsNoTracking()
                .Include(c => c.MonHoc)
                .Where(c => c.MaNguoiDung == userId && c.NgayBatDau <= tomorrow && c.NgayKetThuc >= today)
                .OrderBy(c => c.NgayBatDau)
                .Take(5)
                .Select(c => new DashboardClassScheduleItemDto
                {
                    MaLichHoc = c.MaLichHoc,
                    TenMonHoc = c.MonHoc != null ? c.MonHoc.TenMonHoc : "Môn học",
                    PhongHoc = c.PhongHoc ?? string.Empty,
                    GiangVien = c.GiangVien ?? string.Empty,
                    NgayBatDau = c.NgayBatDau,
                    NgayKetThuc = c.NgayKetThuc,
                    MauSac = c.MauSac ?? "#0EA5E9"
                })
                .ToListAsync();

            // 6. Nearest Exam Schedule
            var nearestExams = await _examScheduleRepository.GetQueryable()
                .AsNoTracking()
                .Include(e => e.MonHoc)
                .Where(e => e.MaNguoiDung == userId && e.NgayThi >= DateTime.UtcNow)
                .OrderBy(e => e.NgayThi)
                .Take(5)
                .Select(e => new DashboardExamScheduleItemDto
                {
                    MaLichThi = e.MaLichThi,
                    TenMonHoc = e.MonHoc != null ? e.MonHoc.TenMonHoc : "Môn học",
                    HinhThucThi = e.HinhThucThi ?? string.Empty,
                    NgayThi = e.NgayThi,
                    ThoiLuong = e.ThoiLuong,
                    PhongThi = e.PhongThi ?? string.Empty
                })
                .ToListAsync();

            // 7. Recent Study Groups
            var groupIds = await _groupMemberRepository.GetQueryable()
                .AsNoTracking()
                .Where(m => m.MaNguoiDung == userId && m.TrangThai == 1)
                .Select(m => m.MaNhom)
                .ToListAsync();

            var recentGroups = await _groupRepository.GetQueryable()
                .AsNoTracking()
                .Include(g => g.ThanhVienNhom)
                .Where(g => groupIds.Contains(g.MaNhom))
                .OrderByDescending(g => g.NgayTao)
                .Take(5)
                .Select(g => new DashboardStudyGroupItemDto
                {
                    MaNhom = g.MaNhom,
                    TenNhom = g.TenNhom,
                    MoTa = g.MoTa,
                    SoThanhVien = g.ThanhVienNhom.Count(m => m.TrangThai == 1),
                    AnhBia = g.AnhDaiDien
                })
                .ToListAsync();

            // 8. Latest Documents
            var latestDocs = await _documentRepository.GetQueryable()
                .AsNoTracking()
                .Include(d => d.FileTaiLen)
                .Include(d => d.NhomHocTap).ThenInclude(g => g.MonHoc)
                .Where(d => d.MaNguoiTaiLen == userId && !d.DaXoa)
                .OrderByDescending(d => d.NgayTaiLen)
                .Take(5)
                .Select(d => new DashboardDocumentItemDto
                {
                    MaTaiLieu = d.MaTaiLieu,
                    TenTaiLieu = d.TieuDe,
                    TenMonHoc = d.NhomHocTap != null && d.NhomHocTap.MonHoc != null ? d.NhomHocTap.MonHoc.TenMonHoc : null,
                    LoaiFile = d.FileTaiLen != null ? d.FileTaiLen.LoaiFile : string.Empty,
                    NgayTải = d.NgayTaiLen
                })
                .ToListAsync();

            // 9. Latest Notifications
            var latestNotifs = await _notificationRepository.GetQueryable()
                .AsNoTracking()
                .Include(n => n.LoaiThongBao)
                .Where(n => n.MaNguoiDung == userId && !n.DaXoa)
                .OrderByDescending(n => n.NgayGui)
                .Take(5)
                .Select(n => new DashboardNotificationItemDto
                {
                    MaThongBao = n.MaThongBao,
                    TieuDe = n.TieuDe,
                    NoiDung = n.NoiDung,
                    Icon = n.LoaiThongBao != null ? n.LoaiThongBao.Icon : "pi-bell",
                    NgayGui = n.NgayGui,
                    DaDoc = n.DaDoc
                })
                .ToListAsync();

            return new DashboardDto
            {
                UserProfile = userProfile,
                Statistics = statistics,
                WeeklyProgress = weeklyProgress,
                TodayTasks = todayTasks,
                UpcomingDeadlines = upcomingDeadlines,
                TodayClassSchedules = todayClasses,
                NearestExamSchedules = nearestExams,
                RecentStudyGroups = recentGroups,
                LatestDocuments = latestDocs,
                LatestNotifications = latestNotifs
            };
        }
    }
}
