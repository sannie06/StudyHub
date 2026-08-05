using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Notification;
using StudyHub.Domain.Entities;
using StudyHub.Persistence;

namespace StudyHub.Web.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckUpcomingAndOverdueTasksAsync();
                    await CheckUpcomingSchedulesAndExamsAsync();
                    await CheckUpcomingGroupMeetingsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Notification Background Service execution cycle.");
                }

                // Run check every 2 minutes
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }

        private async Task CheckUpcomingAndOverdueTasksAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StudyHubDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.Now;
            var in24Hours = now.AddHours(24);

            // 1. Personal Tasks (CongViec) nearing deadline within 24h
            var upcomingTasks = await dbContext.CongViec
                .AsNoTracking()
                .Where(t => !t.DaXoa && t.TrangThai != 3 && t.HanHoanThanh.HasValue &&
                            t.HanHoanThanh.Value >= now && t.HanHoanThanh.Value <= in24Hours)
                .ToListAsync();

            foreach (var task in upcomingTasks)
            {
                bool alreadyNotified = await dbContext.ThongBao.AnyAsync(n =>
                    n.MaNguoiDung == task.MaNguoiDung &&
                    !n.DaXoa &&
                    n.TieuDe.Contains("Cảnh báo Deadline") &&
                    n.NoiDung.Contains(task.TieuDe) &&
                    n.NgayGui >= now.Date);

                if (!alreadyNotified)
                {
                    try
                    {
                        await notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            MaNguoiDung = task.MaNguoiDung,
                            MaLoaiThongBao = 1, // Công việc
                            TieuDe = $"Cảnh báo Deadline: {task.TieuDe}",
                            NoiDung = $"Công việc \"{task.TieuDe}\" sẽ đến hạn vào {task.HanHoanThanh.Value:HH:mm dd/MM/yyyy}!",
                            DuongDan = "/tasks",
                            MucDo = 2
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not send deadline notification for Task {TaskId}", task.MaCongViec);
                    }
                }
            }

            // 2. Overdue Personal Tasks
            var overdueTasks = await dbContext.CongViec
                .AsNoTracking()
                .Where(t => !t.DaXoa && t.TrangThai != 3 && t.HanHoanThanh.HasValue && t.HanHoanThanh.Value < now)
                .ToListAsync();

            foreach (var task in overdueTasks)
            {
                bool alreadyNotifiedOverdue = await dbContext.ThongBao.AnyAsync(n =>
                    n.MaNguoiDung == task.MaNguoiDung &&
                    !n.DaXoa &&
                    n.TieuDe.Contains("Cảnh báo Công việc quá hạn") &&
                    n.NoiDung.Contains(task.TieuDe) &&
                    n.NgayGui >= now.Date);

                if (!alreadyNotifiedOverdue)
                {
                    try
                    {
                        await notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            MaNguoiDung = task.MaNguoiDung,
                            MaLoaiThongBao = 1,
                            TieuDe = $"Cảnh báo Công việc quá hạn: {task.TieuDe}",
                            NoiDung = $"Công việc \"{task.TieuDe}\" đã quá hạn. Hãy kiểm tra và cập nhật tiến độ ngay!",
                            DuongDan = "/tasks",
                            MucDo = 2
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not send overdue notification for Task {TaskId}", task.MaCongViec);
                    }
                }
            }
        }

        private async Task CheckUpcomingSchedulesAndExamsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StudyHubDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.Now;
            var today = now.Date;
            var tomorrow = today.AddDays(1);
            byte todayDayOfWeek = (byte)(now.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)now.DayOfWeek + 1);

            // A. Check Class Schedules (LichHoc) for Today
            var todayClasses = await dbContext.LichHoc
                .AsNoTracking()
                .Include(l => l.MonHoc)
                .Where(l => l.NgayBatDau.Date <= today && l.NgayKetThuc.Date >= today && l.Thu == todayDayOfWeek)
                .ToListAsync();

            foreach (var classSchedule in todayClasses)
            {
                var tenMon = classSchedule.MonHoc?.TenMonHoc ?? classSchedule.TieuDe ?? "Lịch học";
                bool alreadyNotified = await dbContext.ThongBao.AnyAsync(n =>
                    n.MaNguoiDung == classSchedule.MaNguoiDung &&
                    !n.DaXoa &&
                    n.TieuDe.Contains("Sắp đến lịch học") &&
                    n.NoiDung.Contains(tenMon) &&
                    n.NgayGui >= today);

                if (!alreadyNotified)
                {
                    try
                    {
                        var phong = !string.IsNullOrWhiteSpace(classSchedule.PhongHoc) ? classSchedule.PhongHoc : "Phòng học A101";
                        await notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            MaNguoiDung = classSchedule.MaNguoiDung,
                            MaLoaiThongBao = 2, // Lịch học - Lịch thi
                            TieuDe = $"Sắp đến lịch học: Môn {tenMon}",
                            NoiDung = $"Bạn có lớp \"{tenMon}\" ({phong}) diễn ra vào hôm nay.",
                            DuongDan = "/calendar",
                            MucDo = 1
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not send upcoming class notification for ClassSchedule {Id}", classSchedule.MaLichHoc);
                    }
                }
            }

            // B. Check Personal Events & Class Events created on Calendar (SuKien) for Today & Tomorrow
            var todayEvents = await dbContext.SuKien
                .AsNoTracking()
                .Where(e => e.TrangThai == 1 && (e.ThoiGianBatDau.Date == today || e.ThoiGianBatDau.Date == tomorrow))
                .ToListAsync();

            foreach (var eventItem in todayEvents)
            {
                bool alreadyNotified = await dbContext.ThongBao.AnyAsync(n =>
                    n.MaNguoiDung == eventItem.MaNguoiDung &&
                    !n.DaXoa &&
                    n.TieuDe.Contains("Sắp đến lịch học / sự kiện") &&
                    n.NoiDung.Contains(eventItem.TieuDe) &&
                    n.NgayGui >= today);

                if (!alreadyNotified)
                {
                    try
                    {
                        var dayLabel = eventItem.ThoiGianBatDau.Date == today ? "hôm nay" : "ngày mai";
                        await notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            MaNguoiDung = eventItem.MaNguoiDung,
                            MaLoaiThongBao = 2,
                            TieuDe = $"Sắp đến lịch học / sự kiện: {eventItem.TieuDe}",
                            NoiDung = $"Bạn có lịch \"{eventItem.TieuDe}\" vào {eventItem.ThoiGianBatDau:HH:mm} {dayLabel}.",
                            DuongDan = "/calendar",
                            MucDo = 1
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not send upcoming event notification for SuKien {Id}", eventItem.MaSuKien);
                    }
                }
            }

            // C. Check Upcoming Exams (LichThi) within 48h
            var upcomingExams = await dbContext.LichThi
                .AsNoTracking()
                .Include(t => t.MonHoc)
                .Where(t => t.NgayThi.Date >= today && t.NgayThi.Date <= tomorrow)
                .ToListAsync();

            foreach (var exam in upcomingExams)
            {
                var tenMon = exam.MonHoc?.TenMonHoc ?? exam.TieuDe ?? "Môn học";
                bool alreadyNotified = await dbContext.ThongBao.AnyAsync(n =>
                    n.MaNguoiDung == exam.MaNguoiDung &&
                    !n.DaXoa &&
                    n.TieuDe.Contains("Lịch thi sắp tới") &&
                    n.NoiDung.Contains(tenMon) &&
                    n.NgayGui >= today);

                if (!alreadyNotified)
                {
                    try
                    {
                        var phong = !string.IsNullOrWhiteSpace(exam.PhongThi) ? $"tại {exam.PhongThi}" : string.Empty;
                        await notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            MaNguoiDung = exam.MaNguoiDung,
                            MaLoaiThongBao = 2, // Lịch học - Lịch thi
                            TieuDe = $"Lịch thi sắp tới: Kỳ thi {tenMon}",
                            NoiDung = $"Kỳ thi \"{tenMon}\" ({exam.HinhThucThi}) sẽ diễn ra vào lúc {exam.NgayThi:HH:mm dd/MM/yyyy} {phong}.",
                            DuongDan = "/calendar",
                            MucDo = 2
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not send exam notification for Exam {Id}", exam.MaLichThi);
                    }
                }
            }
        }

        private async Task CheckUpcomingGroupMeetingsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StudyHubDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.Now;
            var in24Hours = now.AddHours(24);

            var upcomingMeetings = await dbContext.LichHopNhom
                .AsNoTracking()
                .Include(m => m.NhomHocTap)
                .Where(m => m.TrangThai == 1 && !m.DaXoa && m.ThoiGianBatDau >= now && m.ThoiGianBatDau <= in24Hours)
                .ToListAsync();

            foreach (var meeting in upcomingMeetings)
            {
                var members = await dbContext.ThanhVienNhom
                    .AsNoTracking()
                    .Where(m => m.MaNhom == meeting.MaNhom && m.TrangThai == 1)
                    .Select(m => m.MaNguoiDung)
                    .ToListAsync();

                foreach (var userId in members)
                {
                    bool alreadyNotified = await dbContext.ThongBao.AnyAsync(n =>
                        n.MaNguoiDung == userId &&
                        !n.DaXoa &&
                        n.TieuDe.Contains("Lịch họp nhóm") &&
                        n.NoiDung.Contains(meeting.TieuDe) &&
                        n.NgayGui >= now.Date);

                    if (!alreadyNotified)
                    {
                        try
                        {
                            var tenNhom = meeting.NhomHocTap?.TenNhom ?? "Nhóm học tập";
                            await notificationService.CreateNotificationAsync(new CreateNotificationRequest
                            {
                                MaNguoiDung = userId,
                                MaLoaiThongBao = 2,
                                TieuDe = $"Lịch họp nhóm: Nhóm {tenNhom}",
                                NoiDung = $"Nhóm \"{tenNhom}\" có cuộc họp \"{meeting.TieuDe}\" vào lúc {meeting.ThoiGianBatDau:HH:mm dd/MM/yyyy}.",
                                DuongDan = $"/groups/{meeting.MaNhom}",
                                MucDo = 1
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not send meeting notification for Meeting {Id}", meeting.MaLichHop);
                        }
                    }
                }
            }
        }
    }
}
