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
        
        public List<SubjectProgressDto> SubjectProgress { get; set; }
        public List<WeeklyActivityDto> WeeklyActivity { get; set; }
        public List<HeatMapEntryDto> HeatMap { get; set; }
    }

    public class SubjectProgressDto
    {
        public int MaMonHoc { get; set; }
        public string TenMonHoc { get; set; }
        public string MaMon { get; set; }
        public string MauSac { get; set; }
        public int TaskCount { get; set; }
        public decimal Progress { get; set; }
    }

    public class WeeklyActivityDto
    {
        public string DayName { get; set; } // e.g. "T2", "T3" or date string
        public DateTime Date { get; set; }
        public int FocusMinutes { get; set; }
        public int CompletedTasks { get; set; }
    }

    public class HeatMapEntryDto
    {
        public string Date { get; set; } // "yyyy-MM-dd"
        public int Value { get; set; } // focus minutes
    }
}
