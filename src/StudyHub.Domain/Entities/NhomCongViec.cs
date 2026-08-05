using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class NhomCongViec : BaseEntity
    {
        public int MaNhomCongViec { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public string TenNhom { get; set; }
        public string MoTa { get; set; }
        public string MauSac { get; set; }
        public string Icon { get; set; }
        public int? ThuTu { get; set; }
        public byte TrangThai { get; set; } // 1: Active, 0: Inactive

        public int? NguoiTao { get; set; }
        public int? NguoiCapNhat { get; set; }

        // Navigation properties
        public virtual ICollection<CongViec> CongViec { get; set; } = new List<CongViec>();
    }
}
