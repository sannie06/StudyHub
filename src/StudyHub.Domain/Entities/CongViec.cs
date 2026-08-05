using System;
using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class CongViec : BaseEntity
    {
        public int MaCongViec { get; set; }
        
        public int MaNguoiDung { get; set; }
        public virtual NguoiDung NguoiDung { get; set; }

        public int? MaMonHoc { get; set; }
        public virtual MonHoc MonHoc { get; set; }

        public int? MaNhomCongViec { get; set; }
        public virtual NhomCongViec NhomCongViec { get; set; }

        public string TieuDe { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public byte DoUuTien { get; set; } // 0: Thap, 1: Trung binh, 2: Cao, 3: Khan cap
        public byte TrangThai { get; set; } // 0: Chua bat dau, 1: Dang thuc hien, 2: Tam dung, 3: Hoan thanh, 4: Qua han
        
        public DateTime? NgayBatDau { get; set; }
        public DateTime? HanHoanThanh { get; set; }
        public DateTime? NgayHoanThanh { get; set; }
        public int TiLeHoanThanh { get; set; } = 0;
        public string MauSac { get; set; } = string.Empty;
        
        public bool DanhDauQuanTrong { get; set; } = false;
        public bool DanhDauYeuThich { get; set; } = false;
        
        public bool LapLai { get; set; } = false;
        public int? SoLanLap { get; set; }
        public string GhiChu { get; set; } = string.Empty;

        public int? NguoiTao { get; set; }
        public int? NguoiCapNhat { get; set; }

        // Navigation properties
        public virtual ICollection<PomodoroSession> PomodoroSession { get; set; } = new List<PomodoroSession>();
        public virtual KanbanThe KanbanThe { get; set; }
    }
}
