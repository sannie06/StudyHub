using System;

namespace StudyHub.Application.DTOs.Pomodoro
{
    public class PomodoroSessionDto
    {
        public int MaSession { get; set; }
        public int MaNguoiDung { get; set; }
        public int? MaCongViec { get; set; }
        public int? MaMonHoc { get; set; }
        public string TieuDe { get; set; }
        public byte LoaiSession { get; set; } // 0: Focus, 1: Short Break, 2: Long Break
        public int ThoiLuong { get; set; }
        public int SoLanTamDung { get; set; }
        public int TongThoiGianTamDung { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public byte TrangThai { get; set; } // 0: Huy, 1: Hoan thanh, 2: Dang chay
    }

    public class StartPomodoroRequest
    {
        public int? MaCongViec { get; set; }
        public int? MaMonHoc { get; set; }
        public string TieuDe { get; set; }
        public byte LoaiSession { get; set; } // 0: Focus, 1: Short Break, 2: Long Break
        public int ThoiLuong { get; set; } // In minutes
    }

    public class PausePomodoroRequest
    {
        public int TongThoiGianTamDung { get; set; } // In seconds
    }

    public class FinishPomodoroRequest
    {
        public int TongThoiGianTamDung { get; set; } // In seconds
        public int SoLanTamDung { get; set; }
    }
}
