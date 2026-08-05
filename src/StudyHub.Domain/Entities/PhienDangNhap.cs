using System;

namespace StudyHub.Domain.Entities
{
    public class PhienDangNhap
    {
        public int MaPhien { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public string Token { get; set; }
        public string DiaChiIP { get; set; }
        public string TrinhDuyet { get; set; }
        public string ThietBi { get; set; }
        public string ViTri { get; set; }
        
        public DateTime ThoiGianDangNhap { get; set; } = DateTime.Now;
        public DateTime? ThoiGianHetHan { get; set; }
        public DateTime? ThoiGianDangXuat { get; set; }
        public byte TrangThai { get; set; } // 1: Active, 0: Expired/Logged out
    }
}
