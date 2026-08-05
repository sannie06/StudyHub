using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class VaiTroConfiguration : IEntityTypeConfiguration<VaiTro>
    {
        public void Configure(EntityTypeBuilder<VaiTro> builder)
        {
            builder.ToTable("VaiTro");
            builder.HasKey(x => x.MaVaiTro);
            builder.Property(x => x.MaVaiTro).ValueGeneratedOnAdd();

            builder.Property(x => x.TenVaiTro)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.TenVaiTro)
                .IsUnique();

            builder.Property(x => x.MoTa)
                .HasMaxLength(255);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class QuyenConfiguration : IEntityTypeConfiguration<Quyen>
    {
        public void Configure(EntityTypeBuilder<Quyen> builder)
        {
            builder.ToTable("Quyen");
            builder.HasKey(x => x.MaQuyen);
            builder.Property(x => x.MaQuyen).ValueGeneratedOnAdd();

            builder.Property(x => x.TenQuyen)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.MoTa)
                .HasMaxLength(255);

            builder.Property(x => x.Module)
                .HasMaxLength(100);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class NguoiDungConfiguration : IEntityTypeConfiguration<NguoiDung>
    {
        public void Configure(EntityTypeBuilder<NguoiDung> builder)
        {
            builder.ToTable("NguoiDung");
            builder.HasKey(x => x.MaNguoiDung);
            builder.Property(x => x.MaNguoiDung).ValueGeneratedOnAdd();

            builder.Property(x => x.HoTen)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.SoDienThoai)
                .HasMaxLength(20);

            builder.Property(x => x.MatKhauHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.DiaChi)
                .HasMaxLength(255);

            builder.Property(x => x.AnhDaiDien)
                .HasMaxLength(500);

            builder.HasOne(x => x.VaiTro)
                .WithMany(v => v.NguoiDung)
                .HasForeignKey(x => x.MaVaiTro)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class PhienDangNhapConfiguration : IEntityTypeConfiguration<PhienDangNhap>
    {
        public void Configure(EntityTypeBuilder<PhienDangNhap> builder)
        {
            builder.ToTable("PhienDangNhap");
            builder.HasKey(x => x.MaPhien);
            builder.Property(x => x.MaPhien).ValueGeneratedOnAdd();

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.DiaChiIP)
                .HasMaxLength(50);

            builder.Property(x => x.TrinhDuyet)
                .HasMaxLength(255);

            builder.Property(x => x.ThietBi)
                .HasMaxLength(255);

            builder.Property(x => x.ViTri)
                .HasMaxLength(255);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.PhienDangNhap)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class FileTaiLenConfiguration : IEntityTypeConfiguration<FileTaiLen>
    {
        public void Configure(EntityTypeBuilder<FileTaiLen> builder)
        {
            builder.ToTable("FileTaiLen");
            builder.HasKey(x => x.MaFile);
            builder.Property(x => x.MaFile).ValueGeneratedOnAdd();

            builder.Property(x => x.TenGoc)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.TenLuu)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.DuongDan)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.LoaiFile)
                .HasMaxLength(50);

            builder.Property(x => x.Extension)
                .HasMaxLength(20);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.FileTaiLen)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class OTPConfiguration : IEntityTypeConfiguration<OTP>
    {
        public void Configure(EntityTypeBuilder<OTP> builder)
        {
            builder.ToTable("OTP");
            builder.HasKey(x => x.MaOTP);
            builder.Property(x => x.MaOTP).ValueGeneratedOnAdd();

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.LoaiOTP)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshToken");
            builder.HasKey(x => x.MaToken);
            builder.Property(x => x.MaToken).ValueGeneratedOnAdd();

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }
}
