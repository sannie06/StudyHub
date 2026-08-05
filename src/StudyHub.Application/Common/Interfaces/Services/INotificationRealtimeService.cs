using System.Threading.Tasks;
using StudyHub.Application.DTOs.Notification;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface INotificationRealtimeService
    {
        Task SendNotificationToUserAsync(int userId, ThongBaoDto notification);
    }
}
