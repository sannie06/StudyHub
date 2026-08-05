using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Kanban;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    public class KanbanController : ApiControllerBase
    {
        private readonly IKanbanService _kanbanService;
        private readonly ICurrentUserService _currentUserService;

        public KanbanController(
            IKanbanService kanbanService,
            ICurrentUserService currentUserService)
        {
            _kanbanService = kanbanService;
            _currentUserService = currentUserService;
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

        [HttpGet("boards")]
        public async Task<IActionResult> GetBoards()
        {
            var userId = GetCurrentUserId();
            var boards = await _kanbanService.GetBoardsAsync(userId);
            return Ok(boards);
        }

        [HttpGet("boards/{id}")]
        public async Task<IActionResult> GetBoardDetails(int id)
        {
            var userId = GetCurrentUserId();
            var board = await _kanbanService.GetBoardDetailsAsync(id, userId);
            return Ok(board);
        }

        [HttpPut("cards/move")]
        public async Task<IActionResult> MoveCards([FromBody] MoveCardRequest request)
        {
            var userId = GetCurrentUserId();
            await _kanbanService.MoveCardsAsync(userId, request);
            return NoContent();
        }

        [HttpPost("boards/{id}/columns")]
        public async Task<IActionResult> CreateColumn(int id, [FromBody] CreateColumnRequest request)
        {
            var column = await _kanbanService.CreateColumnAsync(id, request.TenCot, request.MauSac);
            return Created(string.Empty, column);
        }

        [HttpDelete("columns/{id}")]
        public async Task<IActionResult> DeleteColumn(int id)
        {
            await _kanbanService.DeleteColumnAsync(id);
            return NoContent();
        }
    }

    public class CreateColumnRequest
    {
        public string TenCot { get; set; } = null!;
        public string? MauSac { get; set; }
    }
}
