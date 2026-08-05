using System.Collections.Generic;
using System.Threading.Tasks;
using StudyHub.Application.DTOs.Chat;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IChatService
    {
        Task<IEnumerable<TinNhanDto>> GetGroupMessagesAsync(int groupId, int userId, int page = 1, int pageSize = 50);
        Task<TinNhanDto> SendMessageAsync(int userId, SendChatMessageRequest request);
        Task DeleteMessageAsync(int messageId, int userId);
    }
}
