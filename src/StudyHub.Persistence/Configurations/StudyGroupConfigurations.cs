using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class NhomHocTapConfiguration : IEntityTypeConfiguration<NhomHocTap>
    {
        public void Configure(EntityTypeBuilder<NhomHocTap> builder)
        {
            builder.ToTable("NhomHocTap");
            builder.HasKey(x => x.MaNhom);
            builder.Property(x => x.MaNhom).ValueGeneratedOnAdd();

            builder.Property(x => x.TenNhom)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.MoTa)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.AnhDaiDien)
                .HasMaxLength(500);

            builder.Property(x => x.MaThamGia)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(x => x.MaThamGia)
                .IsUnique();

            builder.HasOne(x => x.NguoiTao)
                .WithMany(n => n.NhomHocTap)
                .HasForeignKey(x => x.MaNguoiTao)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MonHoc)
                .WithMany(m => m.NhomHocTap)
                .HasForeignKey(x => x.MaMonHoc)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class ThanhVienNhomConfiguration : IEntityTypeConfiguration<ThanhVienNhom>
    {
        public void Configure(EntityTypeBuilder<ThanhVienNhom> builder)
        {
            builder.ToTable("ThanhVienNhom");
            builder.HasKey(x => x.MaThanhVien);
            builder.Property(x => x.MaThanhVien).ValueGeneratedOnAdd();

            builder.HasOne(x => x.NhomHocTap)
                .WithMany(n => n.ThanhVienNhom)
                .HasForeignKey(x => x.MaNhom)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(u => u.ThanhVienNhom)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.MaNhom, x.MaNguoiDung })
                .IsUnique();
        }
    }

    public class TinNhanConfiguration : IEntityTypeConfiguration<TinNhan>
    {
        public void Configure(EntityTypeBuilder<TinNhan> builder)
        {
            builder.ToTable("TinNhan");
            builder.HasKey(x => x.MaTinNhan);
            builder.Property(x => x.MaTinNhan).ValueGeneratedOnAdd();

            builder.Property(x => x.NoiDung)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.NhomHocTap)
                .WithMany(n => n.TinNhan)
                .HasForeignKey(x => x.MaNhom)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NguoiGui)
                .WithMany(u => u.TinNhan)
                .HasForeignKey(x => x.MaNguoiGui)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class TepDinhKemTinNhanConfiguration : IEntityTypeConfiguration<TepDinhKemTinNhan>
    {
        public void Configure(EntityTypeBuilder<TepDinhKemTinNhan> builder)
        {
            builder.ToTable("TepDinhKemTinNhan");
            builder.HasKey(x => x.MaTep);
            builder.Property(x => x.MaTep).ValueGeneratedOnAdd();

            builder.HasOne(x => x.TinNhan)
                .WithMany(m => m.TepDinhKemTinNhan)
                .HasForeignKey(x => x.MaTinNhan)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.FileTaiLen)
                .WithMany(f => f.TepDinhKemTinNhan)
                .HasForeignKey(x => x.MaFile)
                .OnDelete(DeleteBehavior.Restrict); // Prevent SQL Server cascade cycles
        }
    }

    public class TaiLieuConfiguration : IEntityTypeConfiguration<TaiLieu>
    {
        public void Configure(EntityTypeBuilder<TaiLieu> builder)
        {
            builder.ToTable("TaiLieu");
            builder.HasKey(x => x.MaTaiLieu);
            builder.Property(x => x.MaTaiLieu).ValueGeneratedOnAdd();

            builder.Property(x => x.TieuDe)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.MoTa)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.NhomHocTap)
                .WithMany(g => g.TaiLieu)
                .HasForeignKey(x => x.MaNhom)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NguoiTaiLen)
                .WithMany(u => u.TaiLieu)
                .HasForeignKey(x => x.MaNguoiTaiLen)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FileTaiLen)
                .WithMany(f => f.TaiLieu)
                .HasForeignKey(x => x.MaFile)
                .OnDelete(DeleteBehavior.Restrict); // Prevent SQL Server cascade cycles

            builder.HasOne(x => x.ThuMucTaiLieu)
                .WithMany(f => f.TaiLieu)
                .HasForeignKey(x => x.MaThuMuc)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }
}
