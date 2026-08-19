using System;
using System.Collections.Generic;

namespace StudyHub.Application.DTOs.Analytics
{
    public class AnalyticsDto
    {
        public int TotalFocusMinutes { get; set; }
        public int TotalPomodoros { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal TaskCompletionRate { get; set; }
        public int CurrentStreak { get; set; }
        public decimal ProductivityScore { get; set; }
        
        public List<SubjectProgressDto> SubjectProgress { get; set; } = new();
        public List<WeeklyActivityDto> WeeklyActivity { get; set; } = new();
        public List<HeatMapEntryDto> HeatMap { get; set; } = new();
        public List<UpcomingDeadlineDto> UpcomingDeadlines { get; set; } = new();
    }

    public class UpcomingDeadlineDto
    {
        public int MaCongViec { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string TenMonHoc { get; set; } = string.Empty;
        public DateTime? HanHoanThanh { get; set; }
        public int DoUuTien { get; set; } // 0: Thấp, 1: Trung bình, 2: Cao
        public string PriorityLabel { get; set; } = "Trung bình";
        public string DueLabel { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }

    public class SubjectProgressDto
    {
        public int MaMonHoc { get; set; }
        public string TenMonHoc { get; set; } = string.Empty;
        public string MaMon { get; set; } = string.Empty;
        public string MauSac { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public decimal Progress { get; set; }
    }

    public class WeeklyActivityDto
    {
        public string DayName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int FocusMinutes { get; set; }
        public int CompletedTasks { get; set; }
    }

    public class HeatMapEntryDto
    {
        public string Date { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
