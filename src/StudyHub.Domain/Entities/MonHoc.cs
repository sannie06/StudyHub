using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class MonHoc : BaseEntity
    {
        public int MaMonHoc { get; set; }
        public string TenMonHoc { get; set; }
        public string MaMon { get; set; }
        public string MoTa { get; set; }
        public string MauSac { get; set; }
        public string Icon { get; set; }
        public byte TrangThai { get; set; } // 0: Inactive, 1: Active

        // Navigation properties
        public virtual ICollection<CongViec> CongViec { get; set; } = new List<CongViec>();
        public virtual ICollection<LichHoc> LichHoc { get; set; } = new List<LichHoc>();
        public virtual ICollection<LichThi> LichThi { get; set; } = new List<LichThi>();
        public virtual ICollection<NhomHocTap> NhomHocTap { get; set; } = new List<NhomHocTap>();
        public virtual ICollection<PomodoroSession> PomodoroSession { get; set; } = new List<PomodoroSession>();
    }
}
