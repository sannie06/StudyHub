using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyHub.Application.DTOs.Calendar;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface ICalendarService
    {
        Task<IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(int userId, DateTime start, DateTime end, string[]? types = null);
        Task<CalendarEventDto> CreateEventAsync(int userId, CreateCalendarEventRequest request);
        Task<CalendarEventDto> UpdateEventAsync(int eventId, int userId, UpdateCalendarEventRequest request);
        Task DeleteEventAsync(int eventId, int userId, string? eventType = null);
    }
}
