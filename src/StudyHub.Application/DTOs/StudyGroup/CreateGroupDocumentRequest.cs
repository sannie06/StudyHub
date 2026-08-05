using System.ComponentModel.DataAnnotations;

namespace StudyHub.Application.DTOs.StudyGroup
{
    public class CreateGroupDocumentRequest
    {
        [Required]
        [MaxLength(255)]
        public string TieuDe { get; set; }

        public string? MoTa { get; set; }

        public int? MaThuMuc { get; set; }

        [Required]
        public string DuongDanFile { get; set; }

        public string? Extension { get; set; }

        public long DungLuong { get; set; }
    }
}
