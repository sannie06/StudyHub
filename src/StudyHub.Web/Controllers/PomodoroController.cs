using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Pomodoro;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    public class PomodoroController : ApiControllerBase
    {
        private readonly IPomodoroService _pomodoroService;
        private readonly ICurrentUserService _currentUserService;

        public PomodoroController(
            IPomodoroService pomodoroService,
            ICurrentUserService currentUserService)
        {
            _pomodoroService = pomodoroService;
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

        [HttpPost("sessions")]
        public async Task<IActionResult> StartSession([FromBody] StartPomodoroRequest request)
        {
            var userId = GetCurrentUserId();
            var session = await _pomodoroService.StartSessionAsync(userId, request);
            return Created(string.Empty, session);
        }

        [HttpPut("sessions/{id}/pause")]
        public async Task<IActionResult> PauseSession(int id, [FromBody] PausePomodoroRequest request)
        {
            var userId = GetCurrentUserId();
            var session = await _pomodoroService.PauseSessionAsync(id, userId, request);
            return Ok(session);
        }

        [HttpPut("sessions/{id}/finish")]
        public async Task<IActionResult> FinishSession(int id, [FromBody] FinishPomodoroRequest request)
        {
            var userId = GetCurrentUserId();
            var session = await _pomodoroService.FinishSessionAsync(id, userId, request);
            return Ok(session);
        }

        [HttpPut("sessions/{id}/cancel")]
        public async Task<IActionResult> CancelSession(int id)
        {
            var userId = GetCurrentUserId();
            var session = await _pomodoroService.CancelSessionAsync(id, userId);
            return Ok(session);
        }

        [HttpGet("sessions/active")]
        public async Task<IActionResult> GetActiveSession()
        {
            var userId = GetCurrentUserId();
            var session = await _pomodoroService.GetActiveSessionAsync(userId);
            if (session == null)
            {
                return NotFound("Không có phiên Pomodoro nào đang hoạt động.");
            }
            return Ok(session);
        }
    }
}
