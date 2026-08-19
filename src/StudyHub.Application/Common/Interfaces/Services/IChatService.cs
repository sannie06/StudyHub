using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StudyHub.Application.DTOs.Chat;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IChatService
    {
        Task<IEnumerable<TinNhanDto>> GetGroupMessagesAsync(int groupId, int userId, int page = 1, int pageSize = 50);
        Task<TinNhanDto> SendMessageAsync(int userId, SendChatMessageRequest request);
        Task<TinNhanDto> SendFileMessageAsync(int userId, int groupId, IFormFile file, string? content);
        Task DeleteMessageAsync(int messageId, int userId);
        Task<string> GetPinnedAnnouncementAsync(int groupId, int userId);
        Task UpdatePinnedAnnouncementAsync(int groupId, int userId, string announcement);
    }
}
