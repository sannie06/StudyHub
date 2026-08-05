using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using ValidationException = StudyHub.Application.Common.Exceptions.ValidationException;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.StudyGroup;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    [Route("api/v1/groups")]
    public class GroupsController : ApiControllerBase
    {
        private readonly IStudyGroupService _groupService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateStudyGroupRequest> _createValidator;
        private readonly IValidator<UpdateStudyGroupRequest> _updateValidator;
        private readonly IValidator<JoinGroupRequest> _joinValidator;

        public GroupsController(
            IStudyGroupService groupService,
            ICurrentUserService currentUserService,
            IValidator<CreateStudyGroupRequest> createValidator,
            IValidator<UpdateStudyGroupRequest> updateValidator,
            IValidator<JoinGroupRequest> joinValidator)
        {
            _groupService = groupService;
            _currentUserService = currentUserService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _joinValidator = joinValidator;
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
        public async Task<IActionResult> GetMyGroups([FromQuery] string? search)
        {
            var userId = GetCurrentUserId();
            var list = await _groupService.GetMyGroupsAsync(userId, search);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            var userId = GetCurrentUserId();
            var group = await _groupService.GetGroupByIdAsync(id, userId);
            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateStudyGroupRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var group = await _groupService.CreateGroupAsync(userId, request);
            return CreatedAtAction(nameof(GetGroupById), new { id = group.MaNhom }, group);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateStudyGroupRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var group = await _groupService.UpdateGroupAsync(id, userId, request);
            return Ok(group);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var userId = GetCurrentUserId();
            await _groupService.DeleteGroupAsync(id, userId);
            return NoContent();
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinGroupViaCode([FromBody] JoinGroupRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _joinValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var group = await _groupService.JoinGroupViaCodeAsync(userId, request);
            return Ok(group);
        }

        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveGroup(int id)
        {
            var userId = GetCurrentUserId();
            await _groupService.LeaveGroupAsync(id, userId);
            return NoContent();
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetGroupMembers(int id)
        {
            var userId = GetCurrentUserId();
            var members = await _groupService.GetGroupMembersAsync(id, userId);
            return Ok(members);
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] int memberUserId)
        {
            var currentUserId = GetCurrentUserId();
            var member = await _groupService.AddMemberAsync(id, memberUserId, currentUserId);
            return Ok(member);
        }

        [HttpDelete("{id}/members/{memberUserId}")]
        public async Task<IActionResult> RemoveMember(int id, int memberUserId)
        {
            var currentUserId = GetCurrentUserId();
            await _groupService.RemoveMemberAsync(id, memberUserId, currentUserId);
            return NoContent();
        }

        // ── Group Tasks Endpoints ──
        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetGroupTasks(int id)
        {
            var userId = GetCurrentUserId();
            var tasks = await _groupService.GetGroupTasksAsync(id, userId);
            return Ok(tasks);
        }

        [HttpPost("{id}/tasks")]
        public async Task<IActionResult> CreateGroupTask(int id, [FromBody] CreateGroupTaskRequest request)
        {
            var userId = GetCurrentUserId();
            var task = await _groupService.CreateGroupTaskAsync(id, userId, request);
            return Ok(task);
        }

        [HttpPatch("{id}/tasks/{taskId}/status")]
        public async Task<IActionResult> UpdateGroupTaskStatus(int id, int taskId, [FromBody] UpdateGroupTaskStatusRequest request)
        {
            var userId = GetCurrentUserId();
            var task = await _groupService.UpdateGroupTaskStatusAsync(id, taskId, userId, request.TrangThai);
            return Ok(task);
        }

        [HttpDelete("{id}/tasks/{taskId}")]
        public async Task<IActionResult> DeleteGroupTask(int id, int taskId)
        {
            var userId = GetCurrentUserId();
            await _groupService.DeleteGroupTaskAsync(id, taskId, userId);
            return NoContent();
        }

        // ── Group Meetings Endpoints ──
        [HttpGet("{id}/meetings")]
        public async Task<IActionResult> GetGroupMeetings(int id)
        {
            var userId = GetCurrentUserId();
            var meetings = await _groupService.GetGroupMeetingsAsync(id, userId);
            return Ok(meetings);
        }

        [HttpPost("{id}/meetings")]
        public async Task<IActionResult> CreateGroupMeeting(int id, [FromBody] CreateLichHopRequest request)
        {
            var userId = GetCurrentUserId();
            var meeting = await _groupService.CreateGroupMeetingAsync(id, userId, request);
            return Ok(meeting);
        }

        [HttpPut("{id}/meetings/{meetingId}")]
        public async Task<IActionResult> UpdateGroupMeeting(int id, int meetingId, [FromBody] CreateLichHopRequest request)
        {
            var userId = GetCurrentUserId();
            var meeting = await _groupService.UpdateGroupMeetingAsync(id, meetingId, userId, request);
            return Ok(meeting);
        }

        [HttpDelete("{id}/meetings/{meetingId}")]
        public async Task<IActionResult> DeleteGroupMeeting(int id, int meetingId)
        {
            var userId = GetCurrentUserId();
            await _groupService.DeleteGroupMeetingAsync(id, meetingId, userId);
            return NoContent();
        }

        // ── Group Folders & Documents Endpoints ──
        [HttpGet("{id}/folders")]
        public async Task<IActionResult> GetGroupFolders(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var folders = await _groupService.GetGroupFoldersAsync(id, userId);
                return Ok(folders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        [HttpPost("{id}/folders")]
        public async Task<IActionResult> CreateGroupFolder(int id, [FromBody] CreateThuMucRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var folder = await _groupService.CreateGroupFolderAsync(id, userId, request);
                return Ok(folder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}/folders/{folderId}")]
        public async Task<IActionResult> UpdateGroupFolder(int id, int folderId, [FromBody] UpdateThuMucRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var folder = await _groupService.UpdateGroupFolderAsync(id, folderId, userId, request);
                return Ok(folder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        [HttpDelete("{id}/folders/{folderId}")]
        public async Task<IActionResult> DeleteGroupFolder(int id, int folderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _groupService.DeleteGroupFolderAsync(id, folderId, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        [HttpGet("{id}/documents")]
        public async Task<IActionResult> GetGroupDocuments(int id, [FromQuery] int? folderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var documents = await _groupService.GetGroupDocumentsAsync(id, folderId, userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        [HttpPost("{id}/documents")]
        public async Task<IActionResult> CreateGroupDocument(int id, [FromBody] CreateGroupDocumentRequest request)
        {
            var userId = GetCurrentUserId();
            var doc = await _groupService.CreateGroupDocumentAsync(id, userId, request);
            return Ok(doc);
        }

        [HttpDelete("{id}/documents/{documentId}")]
        public async Task<IActionResult> DeleteGroupDocument(int id, int documentId)
        {
            var userId = GetCurrentUserId();
            await _groupService.DeleteGroupDocumentAsync(id, documentId, userId);
            return NoContent();
        }

        [HttpGet("{id}/documents/{documentId}/download")]
        public async Task<IActionResult> DownloadGroupDocument(int id, int documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (fileBytes, contentType, fileName) = await _groupService.DownloadGroupDocumentAsync(id, documentId, userId);
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }
    }
}
