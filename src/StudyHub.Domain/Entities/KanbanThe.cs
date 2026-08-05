using System;

namespace StudyHub.Domain.Entities
{
    public class KanbanThe
    {
        public int MaThe { get; set; }
        
        public int MaCot { get; set; }
        public virtual KanbanCot KanbanCot { get; set; }

        public int MaCongViec { get; set; }
        public virtual CongViec CongViec { get; set; }

        public int ThuTu { get; set; }
        public DateTime? NgayCapNhat { get; set; }
    }
}
