using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class HoiThoaiAIConfiguration : IEntityTypeConfiguration<HoiThoaiAI>
    {
        public void Configure(EntityTypeBuilder<HoiThoaiAI> builder)
        {
            builder.ToTable("HoiThoaiAI");
            builder.HasKey(x => x.MaHoiThoai);
            builder.Property(x => x.MaHoiThoai).ValueGeneratedOnAdd();

            builder.Property(x => x.TieuDe)
                .HasMaxLength(255);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(u => u.HoiThoaiAI)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class TinNhanAIConfiguration : IEntityTypeConfiguration<TinNhanAI>
    {
        public void Configure(EntityTypeBuilder<TinNhanAI> builder)
        {
            builder.ToTable("TinNhanAI");
            builder.HasKey(x => x.MaTinNhanAI);
            builder.Property(x => x.MaTinNhanAI).ValueGeneratedOnAdd();

            builder.Property(x => x.VaiTro)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.NoiDung)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.HoiThoaiAI)
                .WithMany(h => h.TinNhanAI)
                .HasForeignKey(x => x.MaHoiThoai)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class LichSuTomTatConfiguration : IEntityTypeConfiguration<LichSuTomTat>
    {
        public void Configure(EntityTypeBuilder<LichSuTomTat> builder)
        {
            builder.ToTable("LichSuTomTat");
            builder.HasKey(x => x.MaTomTat);
            builder.Property(x => x.MaTomTat).ValueGeneratedOnAdd();

            builder.Property(x => x.NoiDungTomTat)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.NguoiDung)
                .WithMany(u => u.LichSuTomTat)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Restrict); // Prevent SQL Server cascade cycles

            builder.HasOne(x => x.FileTaiLen)
                .WithMany(f => f.LichSuTomTat)
                .HasForeignKey(x => x.MaFile)
                .OnDelete(DeleteBehavior.Restrict); // Prevent SQL Server cascade cycles
        }
    }

    public class LichSuQuizConfiguration : IEntityTypeConfiguration<LichSuQuiz>
    {
        public void Configure(EntityTypeBuilder<LichSuQuiz> builder)
        {
            builder.ToTable("LichSuQuiz");
            builder.HasKey(x => x.MaQuiz);
            builder.Property(x => x.MaQuiz).ValueGeneratedOnAdd();

            builder.Property(x => x.ChuDe)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.DiemSo)
                .HasPrecision(5, 2);

            builder.Property(x => x.NoiDung)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.NguoiDung)
                .WithMany(u => u.LichSuQuiz)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
