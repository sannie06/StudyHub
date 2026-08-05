using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using ValidationException = StudyHub.Application.Common.Exceptions.ValidationException;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Task;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    public class TasksController : ApiControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateTaskRequest> _createValidator;
        private readonly IValidator<UpdateTaskRequest> _updateValidator;

        public TasksController(
            ITaskService taskService,
            ICurrentUserService currentUserService,
            IValidator<CreateTaskRequest> createValidator,
            IValidator<UpdateTaskRequest> updateValidator)
        {
            _taskService = taskService;
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
        public async Task<IActionResult> GetTasks([FromQuery] TaskQueryParameters queryParameters)
        {
            var userId = GetCurrentUserId();
            var pagedTasks = await _taskService.GetTasksAsync(userId, queryParameters);
            return Ok(pagedTasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId);
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var task = await _taskService.CreateTaskAsync(userId, request);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.MaCongViec }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var task = await _taskService.UpdateTaskAsync(id, userId, request);
            return Ok(task);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusRequest request)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.UpdateTaskStatusAsync(id, userId, request.TrangThai);
            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();
            await _taskService.DeleteTaskAsync(id, userId);
            return NoContent();
        }
    }
}
