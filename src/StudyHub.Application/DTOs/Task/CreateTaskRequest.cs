using System;

namespace StudyHub.Application.DTOs.Task
{
    public class CreateTaskRequest
    {
        public int? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public string TieuDe { get; set; } = null!;
        public string? MoTa { get; set; }
        public byte DoUuTien { get; set; }
        public byte TrangThai { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? HanHoanThanh { get; set; }
        public int TiLeHoanThanh { get; set; }
        public string? MauSac { get; set; }
        public bool DanhDauQuanTrong { get; set; }
        public bool DanhDauYeuThich { get; set; }
        public string? GhiChu { get; set; }
    }
}
