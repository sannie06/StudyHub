using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Common.Exceptions;
using ValidationException = StudyHub.Application.Common.Exceptions.ValidationException;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.TaiLieu;

namespace StudyHub.Web.Controllers
{
    [Authorize]
    public class DocumentsController : ApiControllerBase
    {
        private readonly ITaiLieuService _taiLieuService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<UploadDocumentRequest> _uploadValidator;
        private readonly IValidator<UpdateDocumentRequest> _updateValidator;

        public DocumentsController(
            ITaiLieuService taiLieuService,
            ICurrentUserService currentUserService,
            IValidator<UploadDocumentRequest> uploadValidator,
            IValidator<UpdateDocumentRequest> updateValidator)
        {
            _taiLieuService = taiLieuService;
            _currentUserService = currentUserService;
            _uploadValidator = uploadValidator;
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
        public async Task<IActionResult> GetDocuments([FromQuery] int maNhom, [FromQuery] string? search)
        {
            var userId = GetCurrentUserId();
            var list = await _taiLieuService.GetDocumentsAsync(userId, maNhom, search);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            var userId = GetCurrentUserId();
            var doc = await _taiLieuService.GetDocumentByIdAsync(id, userId);
            return Ok(doc);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _uploadValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var doc = await _taiLieuService.UploadDocumentAsync(userId, request);
            return CreatedAtAction(nameof(GetDocumentById), new { id = doc.MaTaiLieu }, doc);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateDocumentRequest request)
        {
            var userId = GetCurrentUserId();
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var doc = await _taiLieuService.UpdateDocumentAsync(id, userId, request);
            return Ok(doc);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = GetCurrentUserId();
            await _taiLieuService.DeleteDocumentAsync(id, userId);
            return NoContent();
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var userId = GetCurrentUserId();
            var (fileStream, contentType, fileName) = await _taiLieuService.DownloadDocumentAsync(id, userId);
            return File(fileStream, contentType, fileName);
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> PreviewDocument(int id)
        {
            var userId = GetCurrentUserId();
            var (fileStream, contentType, fileName) = await _taiLieuService.DownloadDocumentAsync(id, userId);
            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
            return File(fileStream, contentType);
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareDocument(int id)
        {
            var userId = GetCurrentUserId();
            var doc = await _taiLieuService.GetDocumentByIdAsync(id, userId);
            return Ok(new { shareUrl = $"/api/v1/documents/{id}/download", message = "Liên kết chia sẻ tài liệu đã được tạo." });
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetMyGroups()
        {
            var userId = GetCurrentUserId();
            var groups = await _taiLieuService.GetMyGroupsAsync(userId);
            return Ok(groups);
        }
    }
}
