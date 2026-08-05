using System;

namespace StudyHub.Application.DTOs.Calendar
{
    public class CalendarEventDto
    {
        public string Id { get; set; } = string.Empty; // e.g. "Personal_1", "Class_5", "Exam_2", "Task_10"
        public int SourceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string EventType { get; set; } = "PersonalEvent"; // ClassSchedule, ExamSchedule, TaskDeadline, PersonalEvent
        public string Color { get; set; } = "#4F46E5";
        public string Location { get; set; } = string.Empty;
        public int? ReminderMinutes { get; set; }
        public byte Status { get; set; } = 1; // 1: Active, 0: Cancelled/Completed
        public bool IsEditable { get; set; } = false;

        // Extended fields — trả về từng field riêng, không ghép chuỗi vào Description
        public int? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public string? GiangVien { get; set; }
        public string? HinhThucThi { get; set; }
    }
}
