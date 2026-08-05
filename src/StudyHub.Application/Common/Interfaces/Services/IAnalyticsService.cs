using System.Threading.Tasks;
using StudyHub.Application.DTOs.Analytics;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IAnalyticsService
    {
        Task<AnalyticsDto> GetUserAnalyticsAsync(int userId);
    }
}
