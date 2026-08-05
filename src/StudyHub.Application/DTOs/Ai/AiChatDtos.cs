using System.Collections.Generic;

namespace StudyHub.Application.DTOs.Ai
{
    public class AiChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? PromptType { get; set; } // "General", "TodaySchedule", "UpcomingDeadlines", "PriorityTasks", "WorkloadAnalysis"
    }

    public class AiChatResponse
    {
        public string Reply { get; set; } = string.Empty;
        public List<string> ActionSuggestions { get; set; } = new();
        public string? WorkloadLevel { get; set; } // "Low", "Moderate", "High", "Overloaded"
    }

    public class StudyPlanRequest
    {
        public string Goal { get; set; } = string.Empty;
        public int NumberOfDays { get; set; } = 7;
    }

    public class StudyPlanResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Advice { get; set; } = string.Empty;
        public List<StudyPlanItemDto> PlanItems { get; set; } = new();
    }

    public class StudyPlanItemDto
    {
        public string Day { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string FocusArea { get; set; } = string.Empty;
    }
}
