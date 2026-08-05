using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using ValidationException = StudyHub.Application.Common.Exceptions.ValidationException;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Calendar;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    [Route("api/v1/calendar")]
    public class CalendarController : ApiControllerBase
    {
        private readonly ICalendarService _calendarService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateCalendarEventRequest> _createValidator;
        private readonly IValidator<UpdateCalendarEventRequest> _updateValidator;

        public CalendarController(
            ICalendarService calendarService,
            ICurrentUserService currentUserService,
            IValidator<CreateCalendarEventRequest> createValidator,
            IValidator<UpdateCalendarEventRequest> updateValidator)
        {
            _calendarService = calendarService;
            _currentUserService = currentUserService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
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
        public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] string[]? types)
        {
            var userId = GetCurrentUserId();
            if (start == default) start = DateTime.Now.AddMonths(-1);
            if (end == default) end = DateTime.Now.AddMonths(2);

            var events = await _calendarService.GetCalendarEventsAsync(userId, start, end, types);
            return Ok(events);
        }

        [HttpPost("events")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateCalendarEventRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = await _calendarService.CreateEventAsync(userId, request);
            return Ok(result);
        }

        [HttpPut("events/{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateCalendarEventRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = await _calendarService.UpdateEventAsync(id, userId, request);
            return Ok(result);
        }

        [HttpDelete("events/{id}")]
        public async Task<IActionResult> DeleteEvent(int id, [FromQuery] string? type)
        {
            var userId = GetCurrentUserId();
            await _calendarService.DeleteEventAsync(id, userId, type);
            return NoContent();
        }
    }
}
