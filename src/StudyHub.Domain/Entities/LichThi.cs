using System;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class LichThi : BaseEntity
    {
        public int MaLichThi { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public int MaMonHoc { get; set; }
        public virtual MonHoc MonHoc { get; set; }

        public string? TieuDe { get; set; }
        public string? GiangVien { get; set; }
        public string HinhThucThi { get; set; } // Giua ky, Cuoi ky...
        public DateTime NgayThi { get; set; }
        public int? ThoiLuong { get; set; } // Phut
        public string PhongThi { get; set; }
        public string? MauSac { get; set; }
        public string GhiChu { get; set; }
    }
}
