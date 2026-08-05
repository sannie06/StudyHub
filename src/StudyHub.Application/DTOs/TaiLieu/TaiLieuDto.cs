using System;

namespace StudyHub.Application.DTOs.TaiLieu
{
    public class TaiLieuDto
    {
        public int MaTaiLieu { get; set; }
        public int MaNhom { get; set; }
        public int MaNguoiTaiLen { get; set; }
        public string TenNguoiTaiLen { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public int LuotTai { get; set; }
        public DateTime NgayTaiLen { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        
        // File details
        public int MaFile { get; set; }
        public string TenGoc { get; set; } = string.Empty;
        public string LoaiFile { get; set; } = string.Empty;
        public long DungLuong { get; set; }
        public string Extension { get; set; } = string.Empty;
    }
}
