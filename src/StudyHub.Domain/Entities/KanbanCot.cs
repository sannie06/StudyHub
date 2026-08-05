using System;
using System.Collections.Generic;

namespace StudyHub.Domain.Entities
{
    public class KanbanCot
    {
        public int MaCot { get; set; }
        
        public int MaBoard { get; set; }
        public virtual KanbanBoard KanbanBoard { get; set; }

        public string TenCot { get; set; }
        public string MauSac { get; set; }
        public int ThuTu { get; set; }
        public int? GioiHanThe { get; set; }
        
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        // Navigation property
        public virtual ICollection<KanbanThe> KanbanThe { get; set; } = new List<KanbanThe>();
    }
}
