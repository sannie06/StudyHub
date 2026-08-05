using System;

namespace StudyHub.Domain.Entities
{
    public class ThongKeHocTap
    {
        public int MaThongKe { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }
        
        public int TongCongViec { get; set; }
        public int CongViecHoanThanh { get; set; }
        public int CongViecQuaHan { get; set; }
        public int TongPomodoro { get; set; }
        public int TongPhutHoc { get; set; }
        public int SoNgayHocLienTiep { get; set; }
        public decimal TyLeHoanThanh { get; set; }
        public decimal DiemNangSuat { get; set; }
        public DateTime NgayThongKe { get; set; }
    }
}
