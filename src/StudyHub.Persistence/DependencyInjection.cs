using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Persistence.Repositories;

namespace StudyHub.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<StudyHubDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(StudyHubDbContext).Assembly.FullName));
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            // Register repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<INguoiDungRepository, NguoiDungRepository>();
            services.AddScoped<IOTPRepository, OTPRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ITaiLieuRepository, TaiLieuRepository>();
            services.AddScoped<IFileTaiLenRepository, FileTaiLenRepository>();
            services.AddScoped<INhomHocTapRepository, NhomHocTapRepository>();
            services.AddScoped<IThanhVienNhomRepository, ThanhVienNhomRepository>();
            services.AddScoped<ITinNhanRepository, TinNhanRepository>();
            services.AddScoped<IThongBaoRepository, ThongBaoRepository>();
            services.AddScoped<ILoaiThongBaoRepository, LoaiThongBaoRepository>();
            services.AddScoped<ISuKienRepository, SuKienRepository>();
            services.AddScoped<ILichHocRepository, LichHocRepository>();
            services.AddScoped<ILichThiRepository, LichThiRepository>();
            services.AddScoped<ICongViecRepository, CongViecRepository>();
            services.AddScoped<ICongViecNhomRepository, CongViecNhomRepository>();

            return services;
        }
    }
}
