using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Notification;
using StudyHub.Web.Hubs;

namespace StudyHub.Web.Services
{
    public class NotificationRealtimeService : INotificationRealtimeService
    {
        private readonly IHubContext<StudyHubHub> _hubContext;

        public NotificationRealtimeService(IHubContext<StudyHubHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(int userId, ThongBaoDto notification)
        {
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }
    }
}
