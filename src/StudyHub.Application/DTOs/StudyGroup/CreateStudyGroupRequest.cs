namespace StudyHub.Application.DTOs.StudyGroup
{
    public class CreateStudyGroupRequest
    {
        public string TenNhom { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int? MaMonHoc { get; set; }
        public string? AnhDaiDien { get; set; }
        public int SoLuongToiDa { get; set; } = 10;
    }
}
