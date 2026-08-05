using System;
using System.ComponentModel.DataAnnotations;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class CreateLichHopRequest
    {
        [Required]
        [MaxLength(255)]
        public string TieuDe { get; set; }
        
        public string? MoTa { get; set; }

        [Required]
        [MaxLength(100)]
        public string NenTang { get; set; }

        [Required]
        [MaxLength(500)]
        public string DuongDan { get; set; }

        [Required]
        public DateTime ThoiGianBatDau { get; set; }

        [Required]
        public DateTime ThoiGianKetThuc { get; set; }
    }
}
