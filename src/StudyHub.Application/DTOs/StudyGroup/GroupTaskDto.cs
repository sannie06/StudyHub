using System;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class GroupTaskDto
    {
        public int MaCongViec { get; set; }
        public int MaNhomHocTap { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public byte DoUuTien { get; set; } // 0: Thap, 1: Trung binh, 2: Cao
        public byte TrangThai { get; set; } // 0: Chua bat dau (todo), 1: Dang thuc hien (inProgress), 3: Hoan thanh (done)
        public DateTime? NgayBatDau { get; set; }
        public DateTime? HanHoanThanh { get; set; }
        
        public int? MaNguoiDuocGiao { get; set; }
        public string? TenNguoiDuocGiao { get; set; }
        public string? AnhNguoiDuocGiao { get; set; }

        public int NguoiTaoId { get; set; }
        public string? TenNguoiTao { get; set; }
        public string? AnhNguoiTao { get; set; }

        public DateTime NgayTao { get; set; }
    }
}
