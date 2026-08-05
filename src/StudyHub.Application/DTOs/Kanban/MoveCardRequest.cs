using System.Collections.Generic;

namespace StudyHub.Application.DTOs.Kanban
{
    public class MoveCardRequest
    {
        public List<CardPositionDto> CardPositions { get; set; } = new();
    }

    public class CardPositionDto
    {
        public int MaThe { get; set; }
        public int MaCot { get; set; }
        public int ThuTu { get; set; }
        public byte? NewTaskStatus { get; set; }
    }
}
