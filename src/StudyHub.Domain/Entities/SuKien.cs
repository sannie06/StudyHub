using System;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class SuKien : BaseEntity
    {
        public int MaSuKien { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public int? MaMonHoc { get; set; }
        public virtual MonHoc? MonHoc { get; set; }

        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public string? GiangVien { get; set; }
        public string? HinhThucThi { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string DiaDiem { get; set; }
        public string MauSac { get; set; }
        public int? NhacTruoc { get; set; } // Phut nhac truoc
        public bool LapLai { get; set; } = false;
        public byte TrangThai { get; set; } // 1: Active, 0: Cancelled
    }
}
