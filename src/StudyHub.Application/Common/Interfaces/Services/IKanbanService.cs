using System.Collections.Generic;
using System.Threading.Tasks;
using StudyHub.Application.DTOs.Kanban;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IKanbanService
    {
        Task<List<KanbanBoardDto>> GetBoardsAsync(int userId);
        Task<KanbanBoardDto> GetBoardDetailsAsync(int boardId, int userId);
        Task MoveCardsAsync(int userId, MoveCardRequest request);
        Task<KanbanColumnDto> CreateColumnAsync(int boardId, string name, string? color);
        Task DeleteColumnAsync(int columnId);
    }
}
