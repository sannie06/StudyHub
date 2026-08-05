using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    public class AnalyticsController : ApiControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ICurrentUserService _currentUserService;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            ICurrentUserService currentUserService)
        {
            _analyticsService = analyticsService;
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

        [HttpGet]
        public async Task<IActionResult> GetAnalytics()
        {
            var userId = GetCurrentUserId();
            var analytics = await _analyticsService.GetUserAnalyticsAsync(userId);
            return Ok(analytics);
        }
    }
}
