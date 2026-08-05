using System;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class LichHoc : BaseEntity
    {
        public int MaLichHoc { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public int MaMonHoc { get; set; }
        public virtual MonHoc MonHoc { get; set; }

        public string? TieuDe { get; set; }
        public byte Thu { get; set; } // 2: Thu Hai -> 8: Chu Nhat
        public byte TietBatDau { get; set; }
        public byte TietKetThuc { get; set; }
        public string PhongHoc { get; set; }
        public string GiangVien { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string MauSac { get; set; }
        public string GhiChu { get; set; }
    }
}
