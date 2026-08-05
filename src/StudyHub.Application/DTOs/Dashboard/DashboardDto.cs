using System;
using System.Collections.Generic;

namespace StudyHub.Application.DTOs.Dashboard
{
    public class DashboardDto
    {
        public DashboardUserProfileDto UserProfile { get; set; } = new();
        public DashboardStatisticsDto Statistics { get; set; } = new();
        public List<WeeklyProgressDto> WeeklyProgress { get; set; } = new();
        public List<DashboardTaskItemDto> TodayTasks { get; set; } = new();
        public List<DashboardTaskItemDto> UpcomingDeadlines { get; set; } = new();
        public List<DashboardClassScheduleItemDto> TodayClassSchedules { get; set; } = new();
        public List<DashboardExamScheduleItemDto> NearestExamSchedules { get; set; } = new();
        public List<DashboardStudyGroupItemDto> RecentStudyGroups { get; set; } = new();
        public List<DashboardDocumentItemDto> LatestDocuments { get; set; } = new();
        public List<DashboardNotificationItemDto> LatestNotifications { get; set; } = new();
    }

    public class DashboardUserProfileDto
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string VaiTro { get; set; } = "Học sinh";
    }

    public class DashboardStatisticsDto
    {
        public int TongSoMonHoc { get; set; }
        public int TongSoCongViec { get; set; }
        public int CongViecHoanThanh { get; set; }
        public int CongViecChuaHoanThanh { get; set; }
        public int DeadlineHomNay { get; set; }
        public int ThongBaoChuaDoc { get; set; }
    }

    public class WeeklyProgressDto
    {
        public string DayName { get; set; } = string.Empty;
        public int CompletedCount { get; set; }
        public int CreatedCount { get; set; }
    }

    public class DashboardTaskItemDto
    {
        public int MaCongViec { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public byte DoUuTien { get; set; }
        public byte TrangThai { get; set; }
        public int TiLeHoanThanh { get; set; }
        public DateTime? HanHoanThanh { get; set; }
        public string? TenMonHoc { get; set; }
    }

    public class DashboardClassScheduleItemDto
    {
        public int MaLichHoc { get; set; }
        public string TenMonHoc { get; set; } = string.Empty;
        public string PhongHoc { get; set; } = string.Empty;
        public string GiangVien { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string MauSac { get; set; } = string.Empty;
    }

    public class DashboardExamScheduleItemDto
    {
        public int MaLichThi { get; set; }
        public string TenMonHoc { get; set; } = string.Empty;
        public string HinhThucThi { get; set; } = string.Empty;
        public DateTime NgayThi { get; set; }
        public int? ThoiLuong { get; set; }
        public string PhongThi { get; set; } = string.Empty;
    }

    public class DashboardStudyGroupItemDto
    {
        public int MaNhom { get; set; }
        public string TenNhom { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int SoThanhVien { get; set; }
        public string? AnhBia { get; set; }
    }

    public class DashboardDocumentItemDto
    {
        public int MaTaiLieu { get; set; }
        public string TenTaiLieu { get; set; } = string.Empty;
        public string? TenMonHoc { get; set; }
        public string LoaiFile { get; set; } = string.Empty;
        public DateTime NgayTải { get; set; }
    }

    public class DashboardNotificationItemDto
    {
        public int MaThongBao { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string Icon { get; set; } = "pi-bell";
        public DateTime NgayGui { get; set; }
        public bool DaDoc { get; set; }
    }
}
