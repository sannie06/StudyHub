using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Chat;

namespace StudyHub.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;
        private static readonly ConcurrentDictionary<string, int> OnlineConnections = new();

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
            {
                throw new HubException("Người dùng chưa xác thực.");
            }
            return userId;
        }

        private string GetUserName()
        {
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Thành viên";
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            OnlineConnections[Context.ConnectionId] = userId;
            await Clients.Others.SendAsync("UserOnlineStatus", userId, true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (OnlineConnections.TryRemove(Context.ConnectionId, out int userId))
            {
                await Clients.Others.SendAsync("UserOnlineStatus", userId, false);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGroupChat(int groupId)
        {
            var groupName = $"Group_{groupId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} joined SignalR group {GroupName}", Context.ConnectionId, groupName);
        }

        public async Task LeaveGroupChat(int groupId)
        {
            var groupName = $"Group_{groupId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} left SignalR group {GroupName}", Context.ConnectionId, groupName);
        }

        public async Task SendMessage(int groupId, string content)
        {
            var userId = GetUserId();
            var request = new SendChatMessageRequest
            {
                MaNhom = groupId,
                NoiDung = content,
                LoaiTinNhan = 0
            };

            var messageDto = await _chatService.SendMessageAsync(userId, request);
            var groupName = $"Group_{groupId}";

            // Broadcast message to everyone in the group
            await Clients.Group(groupName).SendAsync("ReceiveMessage", messageDto);
        }

        public async Task SendTyping(int groupId, bool isTyping)
        {
            var userId = GetUserId();
            var userName = GetUserName();
            var groupName = $"Group_{groupId}";

            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", new TypingNotificationDto
            {
                MaNhom = groupId,
                MaNguoiDung = userId,
                TenNguoiDung = userName,
                IsTyping = isTyping
            });
        }
    }
}
