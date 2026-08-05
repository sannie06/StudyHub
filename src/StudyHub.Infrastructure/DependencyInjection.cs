using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.Common.Security;
using StudyHub.Infrastructure.Identity;
using StudyHub.Infrastructure.Services;

namespace StudyHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<Security.SmtpSettings>(configuration.GetSection(Security.SmtpSettings.SectionName));
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IKanbanService, KanbanService>();
            services.AddScoped<ICalendarService, CalendarService>();
            services.AddScoped<IPomodoroService, PomodoroService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<ITaiLieuService, TaiLieuService>();
            services.AddScoped<IStudyGroupService, StudyGroupService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddHttpClient<IAiService, AiService>();
            
            return services;
        }
    }
}
