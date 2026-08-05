using FluentValidation;
using StudyHub.Application.DTOs.TaiLieu;

namespace StudyHub.Application.Validators.TaiLieu
{
    public class UpdateDocumentRequestValidator : AbstractValidator<UpdateDocumentRequest>
    {
        public UpdateDocumentRequestValidator()
        {
            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(255).WithMessage("Tiêu đề không được vượt quá 255 ký tự.");
        }
    }
}
