using FluentValidation;
using StudyHub.Application.DTOs.Subject;

namespace StudyHub.Application.Validators.Subject
{
    public class UpdateSubjectRequestValidator : AbstractValidator<UpdateSubjectRequest>
    {
        public UpdateSubjectRequestValidator()
        {
            RuleFor(x => x.TenMonHoc)
                .NotEmpty().WithMessage("Tên môn học không được để trống.")
                .MaximumLength(150).WithMessage("Tên môn học không vượt quá 150 ký tự.");

            RuleFor(x => x.MaMon)
                .NotEmpty().WithMessage("Mã môn học không được để trống.")
                .MaximumLength(50).WithMessage("Mã môn học không vượt quá 50 ký tự.");

            RuleFor(x => x.MauSac)
                .NotEmpty().WithMessage("Màu sắc không được để trống.")
                .Matches(@"^#(?:[0-9a-fA-F]{3}){1,2}$").WithMessage("Màu sắc phải ở định dạng mã HEX (ví dụ: #6366F1).");

            RuleFor(x => x.Icon)
                .NotEmpty().WithMessage("Biểu tượng không được để trống.")
                .MaximumLength(50).WithMessage("Biểu tượng không vượt quá 50 ký tự.");

            RuleFor(x => x.TrangThai)
                .Must(x => x == 0 || x == 1).WithMessage("Trạng thái không hợp lệ (0: Ngưng hoạt động, 1: Hoạt động).");
        }
    }
}
