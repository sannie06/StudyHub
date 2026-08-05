using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class KanbanBoard : BaseEntity
    {
        public int MaBoard { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public string TenBoard { get; set; }
        public string MoTa { get; set; }
        public string MauSac { get; set; }
        public bool MacDinh { get; set; } = false;

        // Navigation property
        public virtual ICollection<KanbanCot> KanbanCot { get; set; } = new List<KanbanCot>();
    }
}
