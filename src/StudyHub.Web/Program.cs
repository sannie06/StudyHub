using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application;
using StudyHub.Application.Common.Security;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure;
using StudyHub.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Bind JwtSettings manually for Authentication setup in Program.cs
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

builder.Services.AddSignalR();
builder.Services.AddScoped<StudyHub.Application.Common.Interfaces.Services.INotificationRealtimeService, StudyHub.Web.Services.NotificationRealtimeService>();
builder.Services.AddHostedService<StudyHub.Web.Services.NotificationBackgroundService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudyHub API", Version = "v1" });
    
    // Add Bearer token authorize button
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Automatically apply EF Core migrations and ensure database columns exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<StudyHubDbContext>();

        // Auto-create database & apply EF Core Migrations automatically on startup
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("[Database] Migration applied & Database created/verified successfully.");

            // Ensure Admin user (admin@studyhub.com) exists with valid BCrypt hash for "123456" and IsEmailConfirmed = true
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var adminUser = dbContext.NguoiDung.FirstOrDefault(u => u.Email == "admin@studyhub.com" || u.MaVaiTro == 1 || u.MaNguoiDung == 1);
            if (adminUser == null)
            {
                adminUser = new NguoiDung
                {
                    HoTen = "System Admin",
                    Email = "admin@studyhub.com",
                    MatKhauHash = hasher.HashPassword("123456"),
                    MaVaiTro = 1,
                    TrangThai = 1,
                    IsEmailConfirmed = true,
                    NgayTao = DateTime.Now
                };
                dbContext.NguoiDung.Add(adminUser);
            }
            else
            {
                adminUser.Email = "admin@studyhub.com";
                adminUser.MatKhauHash = hasher.HashPassword("123456");
                adminUser.IsEmailConfirmed = true;
                adminUser.TrangThai = 1;
                adminUser.MaVaiTro = 1;
                adminUser.DaXoa = false;
            }
            dbContext.SaveChanges();
            Console.WriteLine("[Admin Seed] Password for admin@studyhub.com updated to '123456' & confirmed successfully.");

            // Ensure Sample Study Groups exist
            if (!dbContext.NhomHocTap.Any(g => !g.DaXoa))
            {
                var sampleGroups = new List<NhomHocTap>
                {
                    new NhomHocTap
                    {
                        TenNhom = "Nhóm Lập Trình Web Fullstack (ASP.NET Core & Angular)",
                        MoTa = "Nhóm học tập, trao đổi kinh nghiệm lập trình Web API và Angular Framework.",
                        MaNguoiTao = adminUser.MaNguoiDung,
                        MaThamGia = "WEB2026",
                        SoLuongToiDa = 15,
                        TrangThai = 1,
                        NgayTao = DateTime.Now.AddDays(-20)
                    },
                    new NhomHocTap
                    {
                        TenNhom = "Cấu Trúc Dữ Liệu & Giải Thuật 2026",
                        MoTa = "Ôn tập thuật toán LeetCode, chuẩn bị cho các kỳ thi và phỏng vấn phần mềm.",
                        MaNguoiTao = adminUser.MaNguoiDung,
                        MaThamGia = "ALGO99",
                        SoLuongToiDa = 10,
                        TrangThai = 1,
                        NgayTao = DateTime.Now.AddDays(-15)
                    },
                    new NhomHocTap
                    {
                        TenNhom = "Cơ Sở Dữ Liệu SQL Server Advanced",
                        MoTa = "Thảo luận tối ưu hóa truy vấn SQL, thiết kế DB và Trigger/Stored Procedures.",
                        MaNguoiTao = adminUser.MaNguoiDung,
                        MaThamGia = "SQLDB88",
                        SoLuongToiDa = 12,
                        TrangThai = 1,
                        NgayTao = DateTime.Now.AddDays(-10)
                    },
                    new NhomHocTap
                    {
                        TenNhom = "Tiếng Anh Chuyên Ngành CNTT",
                        MoTa = "Nhóm luyện nói Tiếng Anh IT, giao tiếp và viết CV ứng tuyển doanh nghiệp.",
                        MaNguoiTao = adminUser.MaNguoiDung,
                        MaThamGia = "ENG4IT",
                        SoLuongToiDa = 20,
                        TrangThai = 1,
                        NgayTao = DateTime.Now.AddDays(-5)
                    }
                };

                dbContext.NhomHocTap.AddRange(sampleGroups);
                dbContext.SaveChanges();

                var allUsers = dbContext.NguoiDung.Where(u => !u.DaXoa).ToList();
                foreach (var grp in sampleGroups)
                {
                    foreach (var usr in allUsers)
                    {
                        dbContext.ThanhVienNhom.Add(new ThanhVienNhom
                        {
                            MaNhom = grp.MaNhom,
                            MaNguoiDung = usr.MaNguoiDung,
                            VaiTro = (byte)(usr.MaNguoiDung == grp.MaNguoiTao ? 2 : 0),
                            TrangThai = 1,
                            NgayThamGia = DateTime.Now.AddDays(-2)
                        });
                    }
                }
                dbContext.SaveChanges();
                Console.WriteLine("[Group Seed] 4 Sample Study Groups seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Database Migration Error] " + ex.Message);
        }

        // 0. Ensure CongViecNhom & OTP tables exist
        try
        {
            dbContext.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CongViecNhom')
                BEGIN
                    CREATE TABLE [dbo].[CongViecNhom](
                        [MaCongViecNhom] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [MaNhomHocTap] INT NOT NULL,
                        [MaNguoiTao] INT NOT NULL,
                        [MaNguoiDuocGiao] INT NULL,
                        [TieuDe] NVARCHAR(200) NOT NULL,
                        [MoTa] NVARCHAR(1000) NULL,
                        [DoUuTien] TINYINT NOT NULL DEFAULT 1,
                        [TrangThai] TINYINT NOT NULL DEFAULT 0,
                        [HanHoanThanh] DATETIME2(7) NULL,
                        [NgayTao] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
                        [NgayCapNhat] DATETIME2(7) NULL,
                        [DaXoa] BIT NOT NULL DEFAULT 0
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OTP')
                BEGIN
                    CREATE TABLE [dbo].[OTP](
                        [MaOTP] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Email] NVARCHAR(255) NOT NULL,
                        [Code] NVARCHAR(10) NOT NULL,
                        [NgayHetHan] DATETIME2(7) NOT NULL,
                        [DaSuDung] BIT NOT NULL DEFAULT 0,
                        [LoaiOTP] NVARCHAR(50) NOT NULL,
                        [NgayTao] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
                        [NgayCapNhat] DATETIME2(7) NULL,
                        [DaXoa] BIT NOT NULL DEFAULT 0
                    );
                END
            ");
            Console.WriteLine("[Database Schema Upgrade] Tables CongViecNhom & OTP verified/created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Table Creation Error] " + ex.Message);
        }
        try { dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichHoc') AND name = 'TieuDe') ALTER TABLE LichHoc ADD TieuDe NVARCHAR(255) NULL;"); } catch { }

        // 2. Ensure columns exist on LichThi
        try { dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichThi') AND name = 'TieuDe') ALTER TABLE LichThi ADD TieuDe NVARCHAR(255) NULL;"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichThi') AND name = 'GiangVien') ALTER TABLE LichThi ADD GiangVien NVARCHAR(100) NULL;"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LichThi') AND name = 'MauSac') ALTER TABLE LichThi ADD MauSac NVARCHAR(20) NULL;"); } catch { }

        // 3. Ensure columns exist on SuKien
        try { dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'MaMonHoc') ALTER TABLE SuKien ADD MaMonHoc INT NULL;"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'GiangVien') ALTER TABLE SuKien ADD GiangVien NVARCHAR(100) NULL;"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SuKien') AND name = 'HinhThucThi') ALTER TABLE SuKien ADD HinhThucThi NVARCHAR(100) NULL;"); } catch { }

        // 4. Update data on SuKien
        try
        {
            dbContext.Database.ExecuteSqlRaw(@"
                EXEC sp_executesql N'
                    UPDATE SuKien
                    SET 
                        GiangVien = CASE 
                            WHEN MoTa LIKE N''%Giảng viên:%'' THEN 
                                LTRIM(RTRIM(SUBSTRING(
                                    MoTa, 
                                    CHARINDEX(N''Giảng viên:'', MoTa) + 11,
                                    CASE 
                                        WHEN CHARINDEX(N''|'', MoTa, CHARINDEX(N''Giảng viên:'', MoTa)) > 0 
                                        THEN CHARINDEX(N''|'', MoTa, CHARINDEX(N''Giảng viên:'', MoTa)) - (CHARINDEX(N''Giảng viên:'', MoTa) + 11)
                                        ELSE LEN(MoTa)
                                    END
                                )))
                            ELSE GiangVien 
                        END,
                        HinhThucThi = CASE 
                            WHEN MoTa LIKE N''%Hình thức:%'' THEN 
                                LTRIM(RTRIM(SUBSTRING(
                                    MoTa, 
                                    CHARINDEX(N''Hình thức:'', MoTa) + 10,
                                    CASE 
                                        WHEN CHARINDEX(N''|'', MoTa, CHARINDEX(N''Hình thức:'', MoTa)) > 0 
                                        THEN CHARINDEX(N''|'', MoTa, CHARINDEX(N''Hình thức:'', MoTa)) - (CHARINDEX(N''Hình thức:'', MoTa) + 10)
                                        ELSE LEN(MoTa)
                                    END
                                )))
                            ELSE HinhThucThi 
                        END
                    WHERE MoTa LIKE N''%Giảng viên:%'' OR MoTa LIKE N''%Môn học:%'';

                    UPDATE SuKien SET MoTa = N'''' WHERE MoTa LIKE N''Môn học:%Giảng viên:%'';
                ';
            ");
        }
        catch { }

        // 5. Ensure LichHopNhom table exists
        try
        {
            dbContext.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LichHopNhom')
                BEGIN
                    CREATE TABLE [dbo].[LichHopNhom](
                        [MaLichHop] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [MaNhom] INT NOT NULL,
                        [MaNguoiTao] INT NOT NULL,
                        [TieuDe] NVARCHAR(255) NOT NULL,
                        [MoTa] NVARCHAR(MAX) NULL,
                        [NenTang] NVARCHAR(100) NOT NULL,
                        [DuongDan] NVARCHAR(500) NOT NULL,
                        [ThoiGianBatDau] DATETIME2(7) NOT NULL,
                        [ThoiGianKetThuc] DATETIME2(7) NOT NULL,
                        [TrangThai] TINYINT NOT NULL DEFAULT 1,
                        [NgayTao] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
                        [NgayCapNhat] DATETIME2(7) NULL,
                        [DaXoa] BIT NOT NULL DEFAULT 0,
                        CONSTRAINT [FK_LichHopNhom_NhomHocTap] FOREIGN KEY ([MaNhom]) REFERENCES [NhomHocTap]([MaNhom]),
                        CONSTRAINT [FK_LichHopNhom_NguoiDung] FOREIGN KEY ([MaNguoiTao]) REFERENCES [NguoiDung]([MaNguoiDung])
                    );
                    CREATE INDEX [IX_LichHopNhom_MaNhom] ON [LichHopNhom]([MaNhom]);
                    CREATE INDEX [IX_LichHopNhom_MaNguoiTao] ON [LichHopNhom]([MaNguoiTao]);
                END
            ");
            Console.WriteLine("[Database Schema Upgrade] Table LichHopNhom verified/created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[LichHopNhom Creation Error] " + ex.Message);
        }

        // 6. Ensure ThuMucTaiLieu table exists and TaiLieu has MaThuMuc column
        try
        {
            dbContext.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThuMucTaiLieu')
                BEGIN
                    CREATE TABLE [dbo].[ThuMucTaiLieu](
                        [MaThuMuc] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [MaNhom] INT NOT NULL,
                        [MaNguoiTao] INT NOT NULL,
                        [TenThuMuc] NVARCHAR(255) NOT NULL,
                        [MoTa] NVARCHAR(MAX) NULL,
                        [NgayTao] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
                        [NgayCapNhat] DATETIME2(7) NULL,
                        [DaXoa] BIT NOT NULL DEFAULT 0,
                        CONSTRAINT [FK_ThuMucTaiLieu_NhomHocTap] FOREIGN KEY ([MaNhom]) REFERENCES [NhomHocTap]([MaNhom]),
                        CONSTRAINT [FK_ThuMucTaiLieu_NguoiDung] FOREIGN KEY ([MaNguoiTao]) REFERENCES [NguoiDung]([MaNguoiDung])
                    );
                    CREATE INDEX [IX_ThuMucTaiLieu_MaNhom] ON [ThuMucTaiLieu]([MaNhom]);
                END
            ");

            dbContext.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TaiLieu') AND name = 'MaThuMuc')
                BEGIN
                    ALTER TABLE [TaiLieu] ADD [MaThuMuc] INT NULL;
                    ALTER TABLE [TaiLieu] ADD CONSTRAINT [FK_TaiLieu_ThuMucTaiLieu] FOREIGN KEY ([MaThuMuc]) REFERENCES [ThuMucTaiLieu]([MaThuMuc]);
                END
            ");
            Console.WriteLine("[Database Schema Upgrade] Table ThuMucTaiLieu & MaThuMuc column verified/created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ThuMucTaiLieu Creation Error] " + ex.Message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Database Migration Warning] {ex.Message}");
    }
}

// Configure CORS before global exception handler so error responses include CORS headers
app.UseCors("AllowAngular");

// Register global exception handler middleware
app.UseMiddleware<StudyHub.Web.Middleware.GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyHub API v1"));
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<StudyHub.Web.Hubs.StudyHubHub>("/hubs/studyhub");
app.MapHub<StudyHub.Web.Hubs.ChatHub>("/hubs/chat");

app.Run();
