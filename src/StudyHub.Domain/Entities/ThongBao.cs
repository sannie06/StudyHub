using System;

namespace StudyHub.Domain.Entities
{
    public class ThongBao
    {
        public int MaThongBao { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public int MaLoaiThongBao { get; set; }
        public virtual LoaiThongBao LoaiThongBao { get; set; }

        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string DuongDan { get; set; }
        public bool DaDoc { get; set; } = false;
        public byte MucDo { get; set; } // 0: Thap, 1: Trung binh, 2: Cao
        
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public DateTime? NgayDoc { get; set; }
        public bool DaXoa { get; set; } = false;
    }
}
