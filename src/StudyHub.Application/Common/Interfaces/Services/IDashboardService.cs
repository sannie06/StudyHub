using System.Threading.Tasks;
using StudyHub.Application.DTOs.Dashboard;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync(int userId);
    }
}
