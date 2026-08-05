using System;
using System.Collections.Generic;
using StudyHub.Domain.Common;

namespace StudyHub.Domain.Entities
{
    public class NguoiDung : BaseEntity
    {
        public int MaNguoiDung { get; set; }
        
        public int MaVaiTro { get; set; }
        public virtual VaiTro VaiTro { get; set; } = null!;

        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public string MatKhauHash { get; set; } = null!;
        public DateTime? NgaySinh { get; set; }
        public byte? GioiTinh { get; set; } // 0=Nu, 1=Nam, 2=Khac
        public string? DiaChi { get; set; }
        public string? AnhDaiDien { get; set; }
        public byte TrangThai { get; set; } = 1; // 1: Active, 0: Blocked
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public string? EmailOtpCode { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
        public string? PasswordResetOtp { get; set; }
        public DateTime? ResetOtpExpiresAt { get; set; }
        public DateTime? LanDangNhapCuoi { get; set; }
        
        public int? NguoiTao { get; set; }
        public int? NguoiCapNhat { get; set; }

        // Navigation properties
        public virtual ICollection<PhienDangNhap> PhienDangNhap { get; set; } = new List<PhienDangNhap>();
        public virtual ICollection<FileTaiLen> FileTaiLen { get; set; } = new List<FileTaiLen>();
        public virtual ICollection<NhomCongViec> NhomCongViec { get; set; } = new List<NhomCongViec>();
        public virtual ICollection<CongViec> CongViec { get; set; } = new List<CongViec>();
        public virtual ICollection<KanbanBoard> KanbanBoard { get; set; } = new List<KanbanBoard>();
        public virtual ICollection<LichHoc> LichHoc { get; set; } = new List<LichHoc>();
        public virtual ICollection<LichThi> LichThi { get; set; } = new List<LichThi>();
        public virtual ICollection<SuKien> SuKien { get; set; } = new List<SuKien>();
        public virtual ICollection<PomodoroSession> PomodoroSession { get; set; } = new List<PomodoroSession>();
        public virtual ICollection<ThongKeHocTap> ThongKeHocTap { get; set; } = new List<ThongKeHocTap>();
        public virtual ICollection<NhomHocTap> NhomHocTap { get; set; } = new List<NhomHocTap>();
        public virtual ICollection<ThanhVienNhom> ThanhVienNhom { get; set; } = new List<ThanhVienNhom>();
        public virtual ICollection<TinNhan> TinNhan { get; set; } = new List<TinNhan>();
        public virtual ICollection<TaiLieu> TaiLieu { get; set; } = new List<TaiLieu>();
        public virtual ICollection<ThongBao> ThongBao { get; set; } = new List<ThongBao>();
        public virtual ICollection<HoiThoaiAI> HoiThoaiAI { get; set; } = new List<HoiThoaiAI>();
        public virtual ICollection<LichSuTomTat> LichSuTomTat { get; set; } = new List<LichSuTomTat>();
        public virtual ICollection<LichSuQuiz> LichSuQuiz { get; set; } = new List<LichSuQuiz>();
        public virtual ICollection<NhatKyHeThong> NhatKyHeThong { get; set; } = new List<NhatKyHeThong>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
