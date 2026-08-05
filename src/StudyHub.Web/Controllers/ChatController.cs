using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using ValidationException = StudyHub.Application.Common.Exceptions.ValidationException;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Chat;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    [Route("api/v1/groups")]
    public class ChatController : ApiControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<SendChatMessageRequest> _sendValidator;

        public ChatController(
            IChatService chatService,
            ICurrentUserService currentUserService,
            IValidator<SendChatMessageRequest> sendValidator)
        {
            _chatService = chatService;
            _currentUserService = currentUserService;
            _sendValidator = sendValidator;
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

        [HttpGet("{groupId}/messages")]
        public async Task<IActionResult> GetGroupMessages(int groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var userId = GetCurrentUserId();
            var list = await _chatService.GetGroupMessagesAsync(groupId, userId, page, pageSize);
            return Ok(list);
        }

        [HttpPost("{groupId}/messages")]
        public async Task<IActionResult> SendMessage(int groupId, [FromBody] SendChatMessageRequest request)
        {
            var userId = GetCurrentUserId();
            request.MaNhom = groupId;

            var validationResult = await _sendValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var message = await _chatService.SendMessageAsync(userId, request);
            return Ok(message);
        }

        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = GetCurrentUserId();
            await _chatService.DeleteMessageAsync(messageId, userId);
            return NoContent();
        }
    }
}
