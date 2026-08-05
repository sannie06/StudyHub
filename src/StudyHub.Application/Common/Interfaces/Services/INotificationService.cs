using System.Collections.Generic;
using System.Threading.Tasks;
using StudyHub.Application.DTOs.Notification;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<ThongBaoDto>> GetMyNotificationsAsync(int userId, bool? unreadOnly = null, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync(int userId);
        Task<ThongBaoDto> MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(int notificationId, int userId);
        Task<ThongBaoDto> CreateNotificationAsync(CreateNotificationRequest request);
    }
}
