using System;

namespace StudyHub.Application.DTOs.Calendar
{
    public class UpdateCalendarEventRequest
    {
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string? DiaDiem { get; set; }
        public string? MauSac { get; set; } = "#4F46E5";
        public int? NhacTruoc { get; set; }
        public byte TrangThai { get; set; } = 1;
        public string? EventType { get; set; }

        // Fields riêng — không ghép chuỗi vào MoTa
        public int? MaMonHoc { get; set; }
        public string? GiangVien { get; set; }
        public string? HinhThucThi { get; set; }
    }
}
