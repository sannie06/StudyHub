using System;

namespace StudyHub.Domain.Entities
{
    public class ThanhVienNhom
    {
        public int MaThanhVien { get; set; }
        
        public int MaNhom { get; set; }
        public virtual NhomHocTap NhomHocTap { get; set; }

        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public byte VaiTro { get; set; } // 0: Member, 1: Moderator, 2: Owner
        public DateTime NgayThamGia { get; set; } = DateTime.Now;
        public byte TrangThai { get; set; } // 1: Active, 0: Left, 2: Pending Approval
    }
}
