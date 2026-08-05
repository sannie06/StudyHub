using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class Quyen : BaseEntity
    {
        public int MaQuyen { get; set; }
        public string TenQuyen { get; set; } = null!;
        public string? MoTa { get; set; }
        public string? Module { get; set; }
    }
}
