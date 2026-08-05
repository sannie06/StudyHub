using System;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class CreateGroupTaskRequest
    {
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public byte DoUuTien { get; set; } = 1; // 0: Thap, 1: Trung binh, 2: Cao
        public DateTime? HanHoanThanh { get; set; }
        public int? MaNguoiDuocGiao { get; set; }
        public byte TrangThai { get; set; } = 0; // 0: todo, 1: inProgress, 3: done
    }

    public class UpdateGroupTaskStatusRequest
    {
        public byte TrangThai { get; set; } // 0: todo, 1: inProgress, 3: done
    }
}
