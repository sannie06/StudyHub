using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using ValidationException = StudyHub.Application.Common.Exceptions.ValidationException;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Notification;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    [Route("api/v1/notifications")]
    public class NotificationsController : ApiControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateNotificationRequest> _createValidator;

        public NotificationsController(
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            IValidator<CreateNotificationRequest> createValidator)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _createValidator = createValidator;
        }

        private int GetCurrentUserId()
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                throw new UnauthorizedException("Người dùng chưa được xác thực.");
            }
            return userId.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] bool? unreadOnly, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var list = await _notificationService.GetMyNotificationsAsync(userId, unreadOnly, page, pageSize);
            return Ok(list);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new NotificationCountDto { UnreadCount = count });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationService.MarkAsReadAsync(id, userId);
            return Ok(notification);
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = GetCurrentUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var notification = await _notificationService.CreateNotificationAsync(request);
            return Ok(notification);
        }
    }
}
