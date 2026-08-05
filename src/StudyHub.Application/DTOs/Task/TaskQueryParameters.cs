namespace StudyHub.Application.DTOs.Task
{
    public class TaskQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public byte? Priority { get; set; }
        public byte? Status { get; set; }
        public int? SubjectId { get; set; }
        public string? SortBy { get; set; }
        public string SortDirection { get; set; } = "asc"; // "asc" or "desc"
    }
}
