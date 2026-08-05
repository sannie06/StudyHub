using System;
using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class ThuMucTaiLieu : BaseEntity
    {
        public int MaThuMuc { get; set; }

        public int MaNhom { get; set; }
        public virtual NhomHocTap NhomHocTap { get; set; }

        public int MaNguoiTao { get; set; }
        public virtual NguoiDung NguoiTao { get; set; }

        public string TenThuMuc { get; set; }
        public string? MoTa { get; set; }

        public virtual ICollection<TaiLieu> TaiLieu { get; set; } = new List<TaiLieu>();
    }
}
