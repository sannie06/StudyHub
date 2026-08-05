namespace StudyHub.Application.DTOs.Subject
{
    public class SubjectDto
    {
        public int MaMonHoc { get; set; }
        public string TenMonHoc { get; set; } = null!;
        public string MaMon { get; set; } = null!;
        public string? MoTa { get; set; }
        public string MauSac { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public byte TrangThai { get; set; }
        public int TaskCount { get; set; }
        public int Progress { get; set; }
    }
}
