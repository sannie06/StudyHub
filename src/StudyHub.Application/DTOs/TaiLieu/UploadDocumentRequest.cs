using Microsoft.AspNetCore.Http;

namespace StudyHub.Application.DTOs.TaiLieu
{
    public class UploadDocumentRequest
    {
        public int MaNhom { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
    }
}
