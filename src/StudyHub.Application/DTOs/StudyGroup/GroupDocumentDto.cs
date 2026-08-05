using System;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class GroupDocumentDto
    {
        public int MaTaiLieu { get; set; }
        public int MaNhom { get; set; }
        public int? MaThuMuc { get; set; }
        public string? TenThuMuc { get; set; }
        public int MaNguoiTaiLen { get; set; }
        public string TenNguoiTaiLen { get; set; }
        public string? AvatarNguoiTaiLen { get; set; }
        public string TieuDe { get; set; }
        public string? MoTa { get; set; }
        public string? DuongDanFile { get; set; }
        public string? Extension { get; set; }
        public long DungLuong { get; set; }
        public DateTime NgayTaiLen { get; set; }
    }
}
