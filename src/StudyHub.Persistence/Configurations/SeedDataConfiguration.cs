using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Configurations
{
    public class SeedDataConfiguration : IEntityTypeConfiguration<VaiTro>, IEntityTypeConfiguration<LoaiThongBao>, IEntityTypeConfiguration<NguoiDung>
    {
        public void Configure(EntityTypeBuilder<VaiTro> builder)
        {
            builder.HasData(
                new VaiTro
                {
                    MaVaiTro = 1,
                    TenVaiTro = "Administrator",
                    MoTa = "Quan tri he thong",
                    NgayTao = new DateTime(2026, 1, 1),
                    DaXoa = false
                },
                new VaiTro
                {
                    MaVaiTro = 2,
                    TenVaiTro = "Student",
                    MoTa = "Sinh vien / Nguoi dung",
                    NgayTao = new DateTime(2026, 1, 1),
                    DaXoa = false
                }
            );
        }

        public void Configure(EntityTypeBuilder<LoaiThongBao> builder)
        {
            builder.HasData(
                new LoaiThongBao
                {
                    MaLoaiThongBao = 1,
                    TenLoai = "Deadline",
                    Icon = "bi bi-alarm",
                    MauSac = "text-danger",
                    MoTa = "Thong bao han chot cong viec"
                },
                new LoaiThongBao
                {
                    MaLoaiThongBao = 2,
                    TenLoai = "Reminder",
                    Icon = "bi bi-bell",
                    MauSac = "text-warning",
                    MoTa = "Nhac nho hoc tap, su kien"
                },
                new LoaiThongBao
                {
                    MaLoaiThongBao = 3,
                    TenLoai = "Group",
                    Icon = "bi bi-people",
                    MauSac = "text-primary",
                    MoTa = "Thong bao hoat dong nhom"
                },
                new LoaiThongBao
                {
                    MaLoaiThongBao = 4,
                    TenLoai = "System",
                    Icon = "bi bi-shield-exclamation",
                    MauSac = "text-secondary",
                    MoTa = "Thong bao tu he thong"
                },
                new LoaiThongBao
                {
                    MaLoaiThongBao = 5,
                    TenLoai = "AI",
                    Icon = "bi bi-cpu",
                    MauSac = "text-info",
                    MoTa = "Goi y va bao cao tu AI"
                }
            );
        }

        public void Configure(EntityTypeBuilder<NguoiDung> builder)
        {
            // Seed a default admin user
            // Password hash below is for "Admin@123" using ASP.NET Core Identity PasswordHasher
            builder.HasData(
                new NguoiDung
                {
                    MaNguoiDung = 1,
                    MaVaiTro = 1,
                    HoTen = "System Admin",
                    Email = "admin@studyhub.com",
                    SoDienThoai = "0123456789",
                    MatKhauHash = "AQAAAAIAAYagAAAAEO9gD1yVzH7qKqTjV+UomN+gI6s8D/H8lE3wFvC9W2vVw==", // Dummy hash for seed
                    TrangThai = 1,
                    NgayTao = new DateTime(2026, 1, 1),
                    DaXoa = false
                }
            );
        }
    }
}
