using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class LichHocConfiguration : IEntityTypeConfiguration<LichHoc>
    {
        public void Configure(EntityTypeBuilder<LichHoc> builder)
        {
            builder.ToTable("LichHoc");
            builder.HasKey(x => x.MaLichHoc);
            builder.Property(x => x.MaLichHoc).ValueGeneratedOnAdd();

            builder.Property(x => x.TieuDe)
                .HasMaxLength(255);

            builder.Property(x => x.PhongHoc)
                .HasMaxLength(100);

            builder.Property(x => x.GiangVien)
                .HasMaxLength(100);

            builder.Property(x => x.MauSac)
                .HasMaxLength(20);

            builder.Property(x => x.GhiChu)
                .HasMaxLength(500);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.LichHoc)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MonHoc)
                .WithMany(m => m.LichHoc)
                .HasForeignKey(x => x.MaMonHoc)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class LichThiConfiguration : IEntityTypeConfiguration<LichThi>
    {
        public void Configure(EntityTypeBuilder<LichThi> builder)
        {
            builder.ToTable("LichThi");
            builder.HasKey(x => x.MaLichThi);
            builder.Property(x => x.MaLichThi).ValueGeneratedOnAdd();

            builder.Property(x => x.TieuDe)
                .HasMaxLength(255);

            builder.Property(x => x.GiangVien)
                .HasMaxLength(100);

            builder.Property(x => x.HinhThucThi)
                .HasMaxLength(100);

            builder.Property(x => x.PhongThi)
                .HasMaxLength(100);

            builder.Property(x => x.MauSac)
                .HasMaxLength(20);

            builder.Property(x => x.GhiChu)
                .HasMaxLength(500);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.LichThi)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MonHoc)
                .WithMany(m => m.LichThi)
                .HasForeignKey(x => x.MaMonHoc)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class SuKienConfiguration : IEntityTypeConfiguration<SuKien>
    {
        public void Configure(EntityTypeBuilder<SuKien> builder)
        {
            builder.ToTable("SuKien");
            builder.HasKey(x => x.MaSuKien);
            builder.Property(x => x.MaSuKien).ValueGeneratedOnAdd();

            builder.Property(x => x.TieuDe)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.MoTa)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.GiangVien)
                .HasMaxLength(100);

            builder.Property(x => x.HinhThucThi)
                .HasMaxLength(100);

            builder.Property(x => x.DiaDiem)
                .HasMaxLength(255);

            builder.Property(x => x.MauSac)
                .HasMaxLength(20);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.SuKien)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MonHoc)
                .WithMany()
                .HasForeignKey(x => x.MaMonHoc)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }
}
