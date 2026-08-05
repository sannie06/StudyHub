namespace StudyHub.Application.DTOs.Subject
{
    public class CreateSubjectRequest
    {
        public string TenMonHoc { get; set; } = null!;
        public string MaMon { get; set; } = null!;
        public string? MoTa { get; set; }
        public string MauSac { get; set; } = null!;
        public string Icon { get; set; } = null!;
    }
}
