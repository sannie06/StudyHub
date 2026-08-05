using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class CongViecNhomConfiguration : IEntityTypeConfiguration<CongViecNhom>
    {
        public void Configure(EntityTypeBuilder<CongViecNhom> builder)
        {
            builder.ToTable("CongViecNhom");
            builder.HasKey(x => x.MaCongViecNhom);

            builder.Property(x => x.TieuDe)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.MoTa)
                .HasMaxLength(1000);

            builder.HasOne(x => x.NhomHocTap)
                .WithMany()
                .HasForeignKey(x => x.MaNhomHocTap)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NguoiTao)
                .WithMany()
                .HasForeignKey(x => x.MaNguoiTao)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.NguoiDuocGiao)
                .WithMany()
                .HasForeignKey(x => x.MaNguoiDuocGiao)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
