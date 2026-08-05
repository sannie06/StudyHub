using FluentValidation;
using StudyHub.Application.DTOs.User;

namespace StudyHub.Application.Validators.User
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.HoTen)
                .NotEmpty().WithMessage("Họ tên không được để trống.")
                .MaximumLength(100).WithMessage("Họ tên không vượt quá 100 ký tự.");

            RuleFor(x => x.SoDienThoai)
                .MaximumLength(15).WithMessage("Số điện thoại không vượt quá 15 ký tự.");

            RuleFor(x => x.GioiTinh)
                .Must(x => !x.HasValue || x.Value == 0 || x.Value == 1 || x.Value == 2)
                .WithMessage("Giới tính không hợp lệ (0: Nữ, 1: Nam, 2: Khác).");
        }
    }
}
