using System;
using System.Collections.Generic;
using StudyHub.Application.DTOs.Dashboard;

namespace StudyHub.Infrastructure.Services.Ai
{
    public class AiContext
    {
        public string Intent { get; set; } = AiIntents.GeneralChat;
        public string RawMessage { get; set; } = string.Empty;
        public string NormalizedQuery { get; set; } = string.Empty;

        // User Identity
        public DashboardUserProfileDto? UserProfile { get; set; }

        // Date Resolution Metadata
        public DateTime? TargetDate { get; set; }
        public string? TargetDateLabel { get; set; }
        public string? DateScope { get; set; }

        // Dedicated Schedule Context (From IAiScheduleContextProvider)
        public List<AiClassScheduleDto>? ScheduleClasses { get; set; }
        public List<AiExamScheduleDto>? ScheduleExams { get; set; }
        public List<AiPersonalEventDto>? ScheduleEvents { get; set; }
        public List<AiTaskScheduleDto>? ScheduleDeadlines { get; set; }

        // Filtered & Relevant Context Properties (Legacy & Dashboard for other intents)
        public DashboardStatisticsDto? Statistics { get; set; }
        public List<DashboardTaskItemDto>? RelevantTasks { get; set; }
        public List<DashboardTaskItemDto>? OverdueTasks { get; set; }
        public List<DashboardTaskItemDto>? UpcomingDeadlines { get; set; }
        public List<DashboardClassScheduleItemDto>? RelevantClassSchedules { get; set; }
        public List<DashboardExamScheduleItemDto>? RelevantExamSchedules { get; set; }
        public List<DashboardDocumentItemDto>? RelevantDocuments { get; set; }

        // Specific Focus Entity
        public DashboardTaskItemDto? FocusedTask { get; set; }
        public string? FocusedTaskTitle { get; set; }
        public DashboardExamScheduleItemDto? FocusedExam { get; set; }

        // Clarification and Guardrail Flags
        public bool IsClarificationNeeded { get; set; } = false;
        public string? ClarificationReason { get; set; }
        public List<string> MissingInformation { get; set; } = new();
        public string? ClarificationPrompt { get; set; }
        public List<string> SuggestedClarificationOptions { get; set; } = new();
    }
}
