using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class NhomHocTap : BaseEntity
    {
        public int MaNhom { get; set; }
        
        public int MaNguoiTao { get; set; }
        public virtual NguoiDung NguoiTao { get; set; }

        public int? MaMonHoc { get; set; }
        public virtual MonHoc MonHoc { get; set; }

        public string TenNhom { get; set; }
        public string MoTa { get; set; }
        public string AnhDaiDien { get; set; }
        public string MaThamGia { get; set; }
        public int SoLuongToiDa { get; set; } = 10;
        public byte TrangThai { get; set; } = 1; // 1: Active, 0: Dissolved

        // Navigation properties
        public virtual ICollection<ThanhVienNhom> ThanhVienNhom { get; set; } = new List<ThanhVienNhom>();
        public virtual ICollection<TinNhan> TinNhan { get; set; } = new List<TinNhan>();
        public virtual ICollection<TaiLieu> TaiLieu { get; set; } = new List<TaiLieu>();
    }
}
