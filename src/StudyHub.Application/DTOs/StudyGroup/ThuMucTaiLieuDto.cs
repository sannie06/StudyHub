using System;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class ThuMucTaiLieuDto
    {
        public int MaThuMuc { get; set; }
        public int MaNhom { get; set; }
        public int MaNguoiTao { get; set; }
        public string TenThuMuc { get; set; }
        public string? MoTa { get; set; }
        public int SoLuongFile { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
