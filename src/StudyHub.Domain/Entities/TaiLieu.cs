using System;

namespace StudyHub.Domain.Entities
{
    public class TaiLieu
    {
        public int MaTaiLieu { get; set; }
        
        public int MaNhom { get; set; }
        public virtual NhomHocTap NhomHocTap { get; set; }

        public int MaNguoiTaiLen { get; set; }
        public virtual NguoiDung NguoiTaiLen { get; set; }

        public int MaFile { get; set; }
        public virtual FileTaiLen FileTaiLen { get; set; }

        public int? MaThuMuc { get; set; }
        public virtual ThuMucTaiLieu? ThuMucTaiLieu { get; set; }

        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public int LuotTai { get; set; } = 0;
        
        public DateTime NgayTaiLen { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }
        public bool DaXoa { get; set; } = false;
    }
}
