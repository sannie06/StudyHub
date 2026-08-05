using System;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class CongViecNhom : BaseEntity
    {
        public int MaCongViecNhom { get; set; }

        public int MaNhomHocTap { get; set; }
        public virtual NhomHocTap NhomHocTap { get; set; }

        public int MaNguoiTao { get; set; }
        public virtual NguoiDung NguoiTao { get; set; }

        public int? MaNguoiDuocGiao { get; set; }
        public virtual NguoiDung NguoiDuocGiao { get; set; }

        public string TieuDe { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public byte DoUuTien { get; set; } // 0: Thap, 1: Trung binh, 2: Cao
        public byte TrangThai { get; set; } // 0: To Do (Can lam), 1: In Progress (Dang lam), 3: Done (Hoan thanh)

        public DateTime? HanHoanThanh { get; set; }
    }
}
