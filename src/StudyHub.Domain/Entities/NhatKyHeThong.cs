using System;

namespace StudyHub.Domain.Entities
{
    public class NhatKyHeThong
    {
        public long MaLog { get; set; }
        
        public int? MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }
        
        public string Module { get; set; }
        public string HanhDong { get; set; }
        public string DiaChiIP { get; set; }
        public string TrinhDuyet { get; set; }
        public DateTime ThoiGian { get; set; } = DateTime.Now;
        public byte MucDo { get; set; } // Info, Warning, Error
    }
}
