using System;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class LichHopNhom : BaseEntity
    {
        public int MaLichHop { get; set; }
        
        public int MaNhom { get; set; }
        public virtual NhomHocTap NhomHocTap { get; set; }

        public int MaNguoiTao { get; set; }
        public virtual NguoiDung NguoiTao { get; set; }

        public string TieuDe { get; set; }
        public string? MoTa { get; set; }
        public string NenTang { get; set; } // Google Meet, Zoom, v.v.
        public string DuongDan { get; set; }
        
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        
        public byte TrangThai { get; set; } = 1; // 1: Active, 0: Cancelled
    }
}
