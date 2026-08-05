using System.Threading.Tasks;
using StudyHub.Application.DTOs.Pomodoro;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IPomodoroService
    {
        Task<PomodoroSessionDto> StartSessionAsync(int userId, StartPomodoroRequest request);
        Task<PomodoroSessionDto> PauseSessionAsync(int id, int userId, PausePomodoroRequest request);
        Task<PomodoroSessionDto> FinishSessionAsync(int id, int userId, FinishPomodoroRequest request);
        Task<PomodoroSessionDto> CancelSessionAsync(int id, int userId);
        Task<PomodoroSessionDto> GetActiveSessionAsync(int userId);
    }
}
