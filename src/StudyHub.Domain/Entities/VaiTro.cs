using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class VaiTro : BaseEntity
    {
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; } = null!;
        public string? MoTa { get; set; }

        // Navigation property
        public virtual ICollection<NguoiDung> NguoiDung { get; set; } = new List<NguoiDung>();
    }
}
