using System;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int MaToken { get; set; }
        public int MaNguoiDung { get; set; }
        public string Token { get; set; } = null!;
        public DateTime NgayHetHan { get; set; }
        public bool DaSuDung { get; set; }
        public bool DaThuHoi { get; set; }

        // Navigation properties
        public virtual NguoiDung NguoiDung { get; set; } = null!;
    }
}
