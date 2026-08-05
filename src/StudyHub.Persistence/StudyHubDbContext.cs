using Microsoft.EntityFrameworkCore;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence
{
    public class StudyHubDbContext : DbContext
    {
        public StudyHubDbContext(DbContextOptions<StudyHubDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        public DbSet<VaiTro> VaiTro { get; set; }
        public DbSet<Quyen> Quyen { get; set; }
        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<PhienDangNhap> PhienDangNhap { get; set; }
        public DbSet<OTP> OTP { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<MonHoc> MonHoc { get; set; }
        public DbSet<FileTaiLen> FileTaiLen { get; set; }
        public DbSet<NhomCongViec> NhomCongViec { get; set; }
        public DbSet<CongViec> CongViec { get; set; }
        public DbSet<CongViecNhom> CongViecNhom { get; set; }
        public DbSet<KanbanBoard> KanbanBoard { get; set; }
        public DbSet<KanbanCot> KanbanCot { get; set; }
        public DbSet<KanbanThe> KanbanThe { get; set; }
        public DbSet<LichHoc> LichHoc { get; set; }
        public DbSet<LichThi> LichThi { get; set; }
        public DbSet<SuKien> SuKien { get; set; }
        public DbSet<PomodoroSession> PomodoroSession { get; set; }
        public DbSet<ThongKeHocTap> ThongKeHocTap { get; set; }
        public DbSet<NhomHocTap> NhomHocTap { get; set; }
        public DbSet<ThanhVienNhom> ThanhVienNhom { get; set; }
        public DbSet<TinNhan> TinNhan { get; set; }
        public DbSet<TepDinhKemTinNhan> TepDinhKemTinNhan { get; set; }
        public DbSet<TaiLieu> TaiLieu { get; set; }
        public DbSet<LoaiThongBao> LoaiThongBao { get; set; }
        public DbSet<ThongBao> ThongBao { get; set; }
        public DbSet<HoiThoaiAI> HoiThoaiAI { get; set; }
        public DbSet<TinNhanAI> TinNhanAI { get; set; }
        public DbSet<LichSuTomTat> LichSuTomTat { get; set; }
        public DbSet<LichSuQuiz> LichSuQuiz { get; set; }
        public DbSet<CauHinhHeThong> CauHinhHeThong { get; set; }
        public DbSet<NhatKyHeThong> NhatKyHeThong { get; set; }
        public DbSet<LichHopNhom> LichHopNhom { get; set; }
        public DbSet<ThuMucTaiLieu> ThuMucTaiLieu { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyHubDbContext).Assembly);
        }
    }
}
