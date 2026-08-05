using System;

namespace StudyHub.Application.DTOs.Notification
{
    public class ThongBaoDto
    {
        public int MaThongBao { get; set; }
        public int MaNguoiDung { get; set; }
        public int MaLoaiThongBao { get; set; }
        public string TenLoaiThongBao { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string MauSac { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string DuongDan { get; set; } = string.Empty;
        public bool DaDoc { get; set; }
        public byte MucDo { get; set; } // 0: Low, 1: Medium, 2: High
        public DateTime NgayGui { get; set; }
        public DateTime? NgayDoc { get; set; }
    }
}
