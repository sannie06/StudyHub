using System.Collections.Generic;

namespace StudyHub.Domain.Entities
{
    public class LoaiThongBao
    {
        public int MaLoaiThongBao { get; set; }
        public string TenLoai { get; set; }
        public string Icon { get; set; }
        public string MauSac { get; set; }
        public string MoTa { get; set; }

        // Navigation property
        public virtual ICollection<ThongBao> ThongBao { get; set; } = new List<ThongBao>();
    }
}
