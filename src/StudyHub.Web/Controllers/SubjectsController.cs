using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using ValidationException = StudyHub.Application.Common.Exceptions.ValidationException;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Subject;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    public class SubjectsController : ApiControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateSubjectRequest> _createValidator;
        private readonly IValidator<UpdateSubjectRequest> _updateValidator;

        public SubjectsController(
            ISubjectService subjectService,
            ICurrentUserService currentUserService,
            IValidator<CreateSubjectRequest> createValidator,
            IValidator<UpdateSubjectRequest> updateValidator)
        {
            _subjectService = subjectService;
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
        public async Task<IActionResult> GetSubjects()
        {
            var userId = GetCurrentUserId();
            var subjects = await _subjectService.GetSubjectsAsync(userId);
            return Ok(subjects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            var userId = GetCurrentUserId();
            var subject = await _subjectService.GetSubjectByIdAsync(id, userId);
            return Ok(subject);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var subject = await _subjectService.CreateSubjectAsync(request);
            return CreatedAtAction(nameof(GetSubjectById), new { id = subject.MaMonHoc }, subject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var subject = await _subjectService.UpdateSubjectAsync(id, request);
            return Ok(subject);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            await _subjectService.DeleteSubjectAsync(id);
            return NoContent();
        }
    }
}
