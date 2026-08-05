using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class ThuMucTaiLieuConfiguration : IEntityTypeConfiguration<ThuMucTaiLieu>
    {
        public void Configure(EntityTypeBuilder<ThuMucTaiLieu> builder)
        {
            builder.ToTable("ThuMucTaiLieu");

            builder.HasKey(e => e.MaThuMuc);

            builder.Property(e => e.TenThuMuc)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasOne(e => e.NhomHocTap)
                .WithMany()
                .HasForeignKey(e => e.MaNhom)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.NguoiTao)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiTao)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
