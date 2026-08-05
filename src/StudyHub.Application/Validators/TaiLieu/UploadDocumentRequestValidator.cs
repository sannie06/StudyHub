using FluentValidation;
using StudyHub.Application.DTOs.TaiLieu;

namespace StudyHub.Application.Validators.TaiLieu
{
    public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
    {
        public UploadDocumentRequestValidator()
        {
            RuleFor(x => x.MaNhom)
                .GreaterThan(0).WithMessage("Mã nhóm học tập không hợp lệ.");

            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(255).WithMessage("Tiêu đề không được vượt quá 255 ký tự.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("Tập tin tải lên không được để trống.");
        }
    }
}
