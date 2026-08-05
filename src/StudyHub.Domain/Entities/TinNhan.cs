using System;
using System.Collections.Generic;

namespace StudyHub.Domain.Entities
{
    public class TinNhan
    {
        public int MaTinNhan { get; set; }
        
        public int MaNhom { get; set; }
        public virtual NhomHocTap NhomHocTap { get; set; }

        public int MaNguoiGui { get; set; }
        public virtual NguoiDung NguoiGui { get; set; }

        public string NoiDung { get; set; }
        public byte LoaiTinNhan { get; set; } // 0: Text, 1: Image, 2: File, 3: System
        public bool DaChinhSua { get; set; } = false;
        
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public DateTime? NgayChinhSua { get; set; }
        public bool DaXoa { get; set; } = false;

        // Navigation properties
        public virtual ICollection<TepDinhKemTinNhan> TepDinhKemTinNhan { get; set; } = new List<TepDinhKemTinNhan>();
    }
}
