using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyHub.Infrastructure.Services.Ai
{
    public class AiClassScheduleDto
    {
        public int MaLichHoc { get; set; }
        public string TenMonHoc { get; set; } = string.Empty;
        public string PhongHoc { get; set; } = string.Empty;
        public string GiangVien { get; set; } = string.Empty;
        public byte Thu { get; set; }
        public byte TietBatDau { get; set; }
        public byte TietKetThuc { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string? MauSac { get; set; }
    }

    public class AiExamScheduleDto
    {
        public int MaLichThi { get; set; }
        public string TenMonHoc { get; set; } = string.Empty;
        public string HinhThucThi { get; set; } = string.Empty;
        public DateTime NgayThi { get; set; }
        public int? ThoiLuong { get; set; }
        public string PhongThi { get; set; } = string.Empty;
        public string? GiangVien { get; set; }
    }

    public class AiPersonalEventDto
    {
        public int MaSuKien { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
    }

    public class AiTaskScheduleDto
    {
        public int MaCongViec { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? TenMonHoc { get; set; }
        public DateTime? HanHoanThanh { get; set; }
        public int DoUuTien { get; set; }
    }

    public class AiScheduleContextResult
    {
        public DateTime TargetStartDate { get; set; }
        public DateTime TargetEndDate { get; set; }
        public string TargetDateLabel { get; set; } = string.Empty;
        public string Scope { get; set; } = "SingleDay";
        public List<AiClassScheduleDto> Classes { get; set; } = new();
        public List<AiExamScheduleDto> Exams { get; set; } = new();
        public List<AiPersonalEventDto> Events { get; set; } = new();
        public List<AiTaskScheduleDto> Deadlines { get; set; } = new();
    }

    public interface IAiScheduleContextProvider
    {
        Task<AiScheduleContextResult> GetScheduleContextAsync(int userId, AiDateResolution dateResolution);
    }
}
