using System.Threading.Tasks;
using StudyHub.Application.DTOs.Common;
using StudyHub.Application.DTOs.Task;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface ITaskService
    {
        Task<PagedList<TaskDto>> GetTasksAsync(int userId, TaskQueryParameters queryParameters);
        Task<TaskDto> GetTaskByIdAsync(int id, int userId);
        Task<TaskDto> CreateTaskAsync(int userId, CreateTaskRequest request);
        Task<TaskDto> UpdateTaskAsync(int id, int userId, UpdateTaskRequest request);
        Task<TaskDto> UpdateTaskStatusAsync(int id, int userId, byte status);
        Task DeleteTaskAsync(int id, int userId);
    }
}
