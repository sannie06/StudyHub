using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class HoiThoaiAI : BaseEntity
    {
        public int MaHoiThoai { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public string TieuDe { get; set; }
        public byte LoaiHoiThoai { get; set; } // 0: Chat, 1: Summary, 2: Quiz...

        // Navigation property
        public virtual ICollection<TinNhanAI> TinNhanAI { get; set; } = new List<TinNhanAI>();
    }
}
