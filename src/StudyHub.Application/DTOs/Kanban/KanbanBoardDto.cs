using System.Collections.Generic;
using StudyHub.Application.DTOs.Task;

namespace StudyHub.Application.DTOs.Kanban
{
    public class KanbanBoardDto
    {
        public int MaBoard { get; set; }
        public string TenBoard { get; set; } = null!;
        public string? MoTa { get; set; }
        public string? MauSac { get; set; }
        public bool MacDinh { get; set; }
        public List<KanbanColumnDto> Columns { get; set; } = new();
    }

    public class KanbanColumnDto
    {
        public int MaCot { get; set; }
        public string TenCot { get; set; } = null!;
        public string? MauSac { get; set; }
        public int ThuTu { get; set; }
        public int? GioiHanThe { get; set; }
        public List<KanbanCardDto> Cards { get; set; } = new();
    }

    public class KanbanCardDto
    {
        public int MaThe { get; set; }
        public int MaCot { get; set; }
        public int MaCongViec { get; set; }
        public int ThuTu { get; set; }
        public TaskDto Task { get; set; } = null!;
    }
}
