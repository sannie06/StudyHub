using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class LichHopNhomConfiguration : IEntityTypeConfiguration<LichHopNhom>
    {
        public void Configure(EntityTypeBuilder<LichHopNhom> builder)
        {
            builder.ToTable("LichHopNhom");

            builder.HasKey(e => e.MaLichHop);

            builder.Property(e => e.TieuDe)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.NenTang)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.DuongDan)
                .IsRequired()
                .HasMaxLength(500);

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
