using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class CauHinhHeThongConfiguration : IEntityTypeConfiguration<CauHinhHeThong>
    {
        public void Configure(EntityTypeBuilder<CauHinhHeThong> builder)
        {
            builder.ToTable("CauHinhHeThong");
            builder.HasKey(x => x.MaCauHinh);
            builder.Property(x => x.MaCauHinh).ValueGeneratedOnAdd();

            builder.Property(x => x.TenCauHinh)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.GiaTri)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.MoTa)
                .HasMaxLength(255);
        }
    }

    public class NhatKyHeThongConfiguration : IEntityTypeConfiguration<NhatKyHeThong>
    {
        public void Configure(EntityTypeBuilder<NhatKyHeThong> builder)
        {
            builder.ToTable("NhatKyHeThong");
            builder.HasKey(x => x.MaLog);
            builder.Property(x => x.MaLog).ValueGeneratedOnAdd();

            builder.Property(x => x.Module)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.HanhDong)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.DiaChiIP)
                .HasMaxLength(50);

            builder.Property(x => x.TrinhDuyet)
                .HasMaxLength(255);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(u => u.NhatKyHeThong)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
