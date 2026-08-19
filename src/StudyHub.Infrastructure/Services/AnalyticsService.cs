using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Analytics;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IGenericRepository<ThongKeHocTap> _statsRepository;
        private readonly IGenericRepository<CongViec> _taskRepository;
        private readonly ISubjectService _subjectService;
        private readonly StudyHub.Persistence.StudyHubDbContext _dbContext;

        public class DayOfWeekHelper
        {
            public static string GetVietnameseDayName(DayOfWeek dayOfWeek)
            {
                return dayOfWeek switch
                {
                    DayOfWeek.Monday => "T2",
                    DayOfWeek.Tuesday => "T3",
                    DayOfWeek.Wednesday => "T4",
                    DayOfWeek.Thursday => "T5",
                    DayOfWeek.Friday => "T6",
                    DayOfWeek.Saturday => "T7",
                    DayOfWeek.Sunday => "CN",
                    _ => ""
                };
            }
        }

        public AnalyticsService(
            IGenericRepository<ThongKeHocTap> statsRepository,
            IGenericRepository<CongViec> taskRepository,
            ISubjectService subjectService,
            StudyHub.Persistence.StudyHubDbContext dbContext)
        {
            _statsRepository = statsRepository;
            _taskRepository = taskRepository;
            _subjectService = subjectService;
            _dbContext = dbContext;
        }

        public async Task<AnalyticsDto> GetUserAnalyticsAsync(int userId)
        {
            // 1. Fetch pre-calculated statistics records & Real Pomodoro Sessions
            var statsRecords = await _statsRepository.GetQueryable()
                .Where(s => s.MaNguoiDung == userId)
                .OrderByDescending(s => s.NgayThongKe)
                .ToListAsync();

            var pomoList = await _dbContext.PomodoroSession
                .AsNoTracking()
                .Where(p => p.MaNguoiDung == userId && p.TrangThai == 1)
                .ToListAsync();

            // Group pomodoro minutes by date
            var pomoByDate = pomoList
                .GroupBy(p => p.ThoiGianBatDau.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.ThoiLuong));

            // 2. Fetch direct task metrics
            var totalTasks = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId);
            var completedTasks = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId && t.TrangThai == 3);
            var overdueTasks = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId && t.TrangThai == 4);

            var completionRate = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

            var statsTotalMinutes = statsRecords.Sum(s => s.TongPhutHoc);
            var pomoTotalMinutes = pomoList.Sum(p => p.ThoiLuong);
            var totalFocusMinutes = Math.Max(statsTotalMinutes, pomoTotalMinutes);

            var statsTotalPomo = statsRecords.Sum(s => s.TongPomodoro);
            var pomoTotalCount = pomoList.Count;
            var totalPomodoros = Math.Max(statsTotalPomo, pomoTotalCount);

            var latestRecord = statsRecords.FirstOrDefault();
            var currentStreak = latestRecord?.SoNgayHocLienTiep ?? (pomoList.Any() ? 1 : 0);
            
            // Check if streak is broken (if last activity was more than 1 day ago)
            if (latestRecord != null && (DateTime.UtcNow.Date - latestRecord.NgayThongKe.Date).TotalDays > 1)
            {
                currentStreak = 0;
            }

            var productivityScore = statsRecords.Sum(s => s.DiemNangSuat);

            // 3. Subject progress
            var subjects = await _subjectService.GetSubjectsAsync(userId);
            var subjectProgressList = subjects.Select(s => new SubjectProgressDto
            {
                MaMonHoc = s.MaMonHoc,
                TenMonHoc = s.TenMonHoc,
                MaMon = s.MaMon,
                MauSac = s.MauSac,
                TaskCount = s.TaskCount,
                Progress = s.Progress
            }).ToList();

            // 4. Weekly Activity (Past 7 days)
            var weeklyActivityList = new List<WeeklyActivityDto>();
            var today = DateTime.UtcNow.Date;
            for (var i = 6; i >= 0; i--)
            {
                var targetDate = today.AddDays(-i);
                var dayRecord = statsRecords.FirstOrDefault(s => s.NgayThongKe.Date == targetDate);
                var pomoMinutesForDay = pomoByDate.TryGetValue(targetDate, out var pm) ? pm : 0;
                var finalFocusMinutes = Math.Max(dayRecord?.TongPhutHoc ?? 0, pomoMinutesForDay);

                weeklyActivityList.Add(new WeeklyActivityDto
                {
                    Date = targetDate,
                    DayName = DayOfWeekHelper.GetVietnameseDayName(targetDate.DayOfWeek),
                    FocusMinutes = finalFocusMinutes,
                    CompletedTasks = dayRecord != null ? dayRecord.CongViecHoanThanh : 0
                });
            }

            // 5. Heat map entries (Past 365 days)
            var heatMapList = new List<HeatMapEntryDto>();
            var oneYearAgo = today.AddDays(-364);

            // Create dictionary for faster lookup combining both stats and pomodoro
            var statsMap = statsRecords
                .Where(s => s.NgayThongKe >= oneYearAgo)
                .GroupBy(s => s.NgayThongKe.Date)
                .ToDictionary(g => g.Key, g => g.First().TongPhutHoc);

            for (var date = oneYearAgo; date <= today; date = date.AddDays(1))
            {
                statsMap.TryGetValue(date, out var studiedMinutes);
                var pomoM = pomoByDate.TryGetValue(date, out var pm) ? pm : 0;
                var finalVal = Math.Max(studiedMinutes, pomoM);
                heatMapList.Add(new HeatMapEntryDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Value = finalVal
                });
            }

            // 6. Upcoming Deadlines (Top 4 closest unfinished tasks)
            var now = DateTime.Now;
            var upcomingTasks = await _taskRepository.GetQueryable()
                .AsNoTracking()
                .Include(t => t.MonHoc)
                .Where(t => t.MaNguoiDung == userId && t.TrangThai != 3 && t.HanHoanThanh != null)
                .OrderBy(t => t.HanHoanThanh)
                .Take(4)
                .ToListAsync();

            var upcomingDeadlineList = upcomingTasks.Select(t =>
            {
                var daysDiff = (t.HanHoanThanh!.Value.Date - now.Date).Days;
                string dueLabel;
                bool isOverdue = false;
                if (daysDiff < 0)
                {
                    dueLabel = $"Quá hạn {Math.Abs(daysDiff)} ngày";
                    isOverdue = true;
                }
                else if (daysDiff == 0)
                {
                    dueLabel = "Hôm nay";
                }
                else if (daysDiff == 1)
                {
                    dueLabel = "Ngày mai";
                }
                else
                {
                    dueLabel = $"{daysDiff} ngày nữa";
                }

                string priorityLabel = t.DoUuTien switch
                {
                    2 => "Cao",
                    0 => "Thấp",
                    _ => "Trung bình"
                };

                return new UpcomingDeadlineDto
                {
                    MaCongViec = t.MaCongViec,
                    TieuDe = t.TieuDe,
                    TenMonHoc = t.MonHoc?.TenMonHoc ?? "Chung",
                    HanHoanThanh = t.HanHoanThanh,
                    DoUuTien = t.DoUuTien,
                    PriorityLabel = priorityLabel,
                    DueLabel = dueLabel,
                    IsOverdue = isOverdue
                };
            }).ToList();

            return new AnalyticsDto
            {
                TotalFocusMinutes = totalFocusMinutes,
                TotalPomodoros = totalPomodoros,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                OverdueTasks = overdueTasks,
                TaskCompletionRate = completionRate,
                CurrentStreak = currentStreak,
                ProductivityScore = productivityScore,
                SubjectProgress = subjectProgressList,
                WeeklyActivity = weeklyActivityList,
                HeatMap = heatMapList,
                UpcomingDeadlines = upcomingDeadlineList
            };
        }
    }
}
