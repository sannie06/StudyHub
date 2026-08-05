using System;

namespace StudyHub.Domain.Entities
{
    public class PomodoroSession
    {
        public int MaSession { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public int? MaCongViec { get; set; }
        public virtual CongViec CongViec { get; set; }

        public int? MaMonHoc { get; set; }
        public virtual MonHoc MonHoc { get; set; }

        public string TieuDe { get; set; }
        public byte LoaiSession { get; set; } // 0: Focus, 1: Short Break, 2: Long Break
        public int ThoiLuong { get; set; } // So phut
        
        public int SoLanTamDung { get; set; } = 0;
        public int TongThoiGianTamDung { get; set; } = 0; // Tinh bang giay
        
        public DateTime ThoiGianBatDau { get; set; } = DateTime.Now;
        public DateTime? ThoiGianKetThuc { get; set; }
        public byte TrangThai { get; set; } // 0: Huy, 1: Hoan thanh, 2: Dang chay
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
