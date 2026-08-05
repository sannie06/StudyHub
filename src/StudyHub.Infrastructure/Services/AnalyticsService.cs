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
            ISubjectService subjectService)
        {
            _statsRepository = statsRepository;
            _taskRepository = taskRepository;
            _subjectService = subjectService;
        }

        public async Task<AnalyticsDto> GetUserAnalyticsAsync(int userId)
        {
            // 1. Fetch pre-calculated statistics records
            var statsRecords = await _statsRepository.GetQueryable()
                .Where(s => s.MaNguoiDung == userId)
                .OrderByDescending(s => s.NgayThongKe)
                .ToListAsync();

            // 2. Fetch direct task metrics
            var totalTasks = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId);
            var completedTasks = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId && t.TrangThai == 3);
            var overdueTasks = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId && t.TrangThai == 4);

            var completionRate = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

            var totalFocusMinutes = statsRecords.Sum(s => s.TongPhutHoc);
            var totalPomodoros = statsRecords.Sum(s => s.TongPomodoro);
            var latestRecord = statsRecords.FirstOrDefault();
            var currentStreak = latestRecord?.SoNgayHocLienTiep ?? 0;
            
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

                weeklyActivityList.Add(new WeeklyActivityDto
                {
                    Date = targetDate,
                    DayName = DayOfWeekHelper.GetVietnameseDayName(targetDate.DayOfWeek),
                    FocusMinutes = dayRecord?.TongPhutHoc ?? 0,
                    CompletedTasks = dayRecord != null ? dayRecord.CongViecHoanThanh : 0
                });
            }

            // 5. Heat map entries (Past 365 days)
            var heatMapList = new List<HeatMapEntryDto>();
            var oneYearAgo = today.AddDays(-364);

            // Create dictionary for faster lookup
            var statsMap = statsRecords
                .Where(s => s.NgayThongKe >= oneYearAgo)
                .GroupBy(s => s.NgayThongKe.Date)
                .ToDictionary(g => g.Key, g => g.First().TongPhutHoc);

            for (var date = oneYearAgo; date <= today; date = date.AddDays(1))
            {
                statsMap.TryGetValue(date, out var studiedMinutes);
                heatMapList.Add(new HeatMapEntryDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Value = studiedMinutes
                });
            }

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
                HeatMap = heatMapList
            };
        }
    }
}
