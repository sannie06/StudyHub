using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Ai;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    [Route("api/v1/ai")]
    public class AiController : ApiControllerBase
    {
        private readonly IAiService _aiService;
        private readonly ICurrentUserService _currentUserService;

        public AiController(
            IAiService aiService,
            ICurrentUserService currentUserService)
        {
            _aiService = aiService;
            _currentUserService = currentUserService;
        }

        private int GetUserId()
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                throw new UnauthorizedException("Người dùng chưa được xác thực.");
            }
            return userId.Value;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiChatRequest request)
        {
            var userId = GetUserId();
            var result = await _aiService.ChatAsync(userId, request);
            return Ok(result);
        }

        [HttpPost("study-plan")]
        public async Task<IActionResult> GenerateStudyPlan([FromBody] StudyPlanRequest request)
        {
            var userId = GetUserId();
            var result = await _aiService.GenerateStudyPlanAsync(userId, request);
            return Ok(result);
        }

        [HttpGet("workload")]
        public async Task<IActionResult> AnalyzeWorkload()
        {
            var userId = GetUserId();
            var analysis = await _aiService.AnalyzeWorkloadAsync(userId);
            return Ok(new { WorkloadAnalysis = analysis });
        }

        [HttpGet("advice")]
        public async Task<IActionResult> GetStudyAdvice()
        {
            var userId = GetUserId();
            var advice = await _aiService.GetStudyAdviceAsync(userId);
            return Ok(new { Advice = advice });
        }
    }
}
