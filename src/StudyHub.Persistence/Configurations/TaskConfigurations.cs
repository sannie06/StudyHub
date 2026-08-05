using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class MonHocConfiguration : IEntityTypeConfiguration<MonHoc>
    {
        public void Configure(EntityTypeBuilder<MonHoc> builder)
        {
            builder.ToTable("MonHoc");
            builder.HasKey(x => x.MaMonHoc);
            builder.Property(x => x.MaMonHoc).ValueGeneratedOnAdd();

            builder.Property(x => x.TenMonHoc)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.MaMon)
                .HasMaxLength(30);

            builder.Property(x => x.MoTa)
                .HasMaxLength(255);

            builder.Property(x => x.MauSac)
                .HasMaxLength(20);

            builder.Property(x => x.Icon)
                .HasMaxLength(100);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class NhomCongViecConfiguration : IEntityTypeConfiguration<NhomCongViec>
    {
        public void Configure(EntityTypeBuilder<NhomCongViec> builder)
        {
            builder.ToTable("NhomCongViec");
            builder.HasKey(x => x.MaNhomCongViec);
            builder.Property(x => x.MaNhomCongViec).ValueGeneratedOnAdd();

            builder.Property(x => x.TenNhom)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.MoTa)
                .HasMaxLength(255);

            builder.Property(x => x.MauSac)
                .HasMaxLength(20);

            builder.Property(x => x.Icon)
                .HasMaxLength(100);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.NhomCongViec)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class CongViecConfiguration : IEntityTypeConfiguration<CongViec>
    {
        public void Configure(EntityTypeBuilder<CongViec> builder)
        {
            builder.ToTable("CongViec");
            builder.HasKey(x => x.MaCongViec);
            builder.Property(x => x.MaCongViec).ValueGeneratedOnAdd();

            builder.Property(x => x.TieuDe)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.MoTa)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.GhiChu)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.MauSac)
                .HasMaxLength(20);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.CongViec)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MonHoc)
                .WithMany(m => m.CongViec)
                .HasForeignKey(x => x.MaMonHoc)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.NhomCongViec)
                .WithMany(nc => nc.CongViec)
                .HasForeignKey(x => x.MaNhomCongViec)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class KanbanBoardConfiguration : IEntityTypeConfiguration<KanbanBoard>
    {
        public void Configure(EntityTypeBuilder<KanbanBoard> builder)
        {
            builder.ToTable("KanbanBoard");
            builder.HasKey(x => x.MaBoard);
            builder.Property(x => x.MaBoard).ValueGeneratedOnAdd();

            builder.Property(x => x.TenBoard)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.MoTa)
                .HasMaxLength(255);

            builder.Property(x => x.MauSac)
                .HasMaxLength(20);

            builder.HasOne(x => x.NguoiDung)
                .WithMany(n => n.KanbanBoard)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(x => !x.DaXoa);
        }
    }

    public class KanbanCotConfiguration : IEntityTypeConfiguration<KanbanCot>
    {
        public void Configure(EntityTypeBuilder<KanbanCot> builder)
        {
            builder.ToTable("KanbanCot");
            builder.HasKey(x => x.MaCot);
            builder.Property(x => x.MaCot).ValueGeneratedOnAdd();

            builder.Property(x => x.TenCot)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.MauSac)
                .HasMaxLength(20);

            builder.HasOne(x => x.KanbanBoard)
                .WithMany(b => b.KanbanCot)
                .HasForeignKey(x => x.MaBoard)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class KanbanTheConfiguration : IEntityTypeConfiguration<KanbanThe>
    {
        public void Configure(EntityTypeBuilder<KanbanThe> builder)
        {
            builder.ToTable("KanbanThe");
            builder.HasKey(x => x.MaThe);
            builder.Property(x => x.MaThe).ValueGeneratedOnAdd();

            builder.HasOne(x => x.KanbanCot)
                .WithMany(c => c.KanbanThe)
                .HasForeignKey(x => x.MaCot)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CongViec)
                .WithOne(cv => cv.KanbanThe)
                .HasForeignKey<KanbanThe>(x => x.MaCongViec)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
