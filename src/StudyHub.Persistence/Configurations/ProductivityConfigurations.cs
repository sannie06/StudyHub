using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class PomodoroSessionConfiguration : IEntityTypeConfiguration<PomodoroSession>
    {
        public void Configure(EntityTypeBuilder<PomodoroSession> builder)
        {
            builder.ToTable("PomodoroSession");
            builder.HasKey(x => x.MaSession);
            builder.Property(x => x.MaSession).ValueGeneratedOnAdd();

            builder.Property(x => x.TieuDe)
                .HasMaxLength(255);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.PomodoroSession)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CongViec)
                .WithMany(c => c.PomodoroSession)
                .HasForeignKey(x => x.MaCongViec)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MonHoc)
                .WithMany(m => m.PomodoroSession)
                .HasForeignKey(x => x.MaMonHoc)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ThongKeHocTapConfiguration : IEntityTypeConfiguration<ThongKeHocTap>
    {
        public void Configure(EntityTypeBuilder<ThongKeHocTap> builder)
        {
            builder.ToTable("ThongKeHocTap");
            builder.HasKey(x => x.MaThongKe);
            builder.Property(x => x.MaThongKe).ValueGeneratedOnAdd();

            builder.Property(x => x.TyLeHoanThanh)
                .HasPrecision(5, 2);

            builder.Property(x => x.DiemNangSuat)
                .HasPrecision(5, 2);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.ThongKeHocTap)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.MaNguoiDung, x.NgayThongKe })
                .IsUnique();
        }
    }
}
