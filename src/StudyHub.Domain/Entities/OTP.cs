using System;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class OTP : BaseEntity
    {
        public int MaOTP { get; set; }
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime NgayHetHan { get; set; }
        public bool DaSuDung { get; set; }
        public string LoaiOTP { get; set; } = null!; // Register, ForgotPassword
    }
}
