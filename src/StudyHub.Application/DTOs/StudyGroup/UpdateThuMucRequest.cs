using System.ComponentModel.DataAnnotations;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class UpdateThuMucRequest
    {
        [Required]
        [MaxLength(255)]
        public string TenThuMuc { get; set; }

        public string? MoTa { get; set; }
    }
}
