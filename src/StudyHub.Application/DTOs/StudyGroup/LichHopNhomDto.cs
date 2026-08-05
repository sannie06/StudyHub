using System;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class LichHopNhomDto
    {
        public int MaLichHop { get; set; }
        public int MaNhom { get; set; }
        public int MaNguoiTao { get; set; }
        public string TenNguoiTao { get; set; }
        public string TieuDe { get; set; }
        public string? MoTa { get; set; }
        public string NenTang { get; set; }
        public string DuongDan { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public byte TrangThai { get; set; }
    }
}
