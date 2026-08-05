using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class LoaiThongBaoConfiguration : IEntityTypeConfiguration<LoaiThongBao>
    {
        public void Configure(EntityTypeBuilder<LoaiThongBao> builder)
        {
            builder.ToTable("LoaiThongBao");
            builder.HasKey(x => x.MaLoaiThongBao);
            builder.Property(x => x.MaLoaiThongBao).ValueGeneratedOnAdd();

            builder.Property(x => x.TenLoai)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.TenLoai)
                .IsUnique();

            builder.Property(x => x.Icon)
                .HasMaxLength(100);

            builder.Property(x => x.MauSac)
                .HasMaxLength(30);

            builder.Property(x => x.MoTa)
                .HasMaxLength(255);
        }
    }

    public class ThongBaoConfiguration : IEntityTypeConfiguration<ThongBao>
    {
        public void Configure(EntityTypeBuilder<ThongBao> builder)
        {
            builder.ToTable("ThongBao");
            builder.HasKey(x => x.MaThongBao);
            builder.Property(x => x.MaThongBao).ValueGeneratedOnAdd();

            builder.Property(x => x.TieuDe)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.NoiDung)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.DuongDan)
                .HasMaxLength(500);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(u => u.ThongBao)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.LoaiThongBao)
                .WithMany(lt => lt.ThongBao)
                .HasForeignKey(x => x.MaLoaiThongBao)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }
}
