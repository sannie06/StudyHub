using FluentValidation;
using StudyHub.Application.DTOs.Notification;

namespace StudyHub.Application.Validators.Notification
{
    public class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
    {
        public CreateNotificationRequestValidator()
        {
            RuleFor(x => x.MaNguoiDung)
                .GreaterThan(0).WithMessage("Mã người dùng nhận thông báo không hợp lệ.");

            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề thông báo không được để trống.")
                .MaximumLength(150).WithMessage("Tiêu đề thông báo không được vượt quá 150 ký tự.");

            RuleFor(x => x.NoiDung)
                .NotEmpty().WithMessage("Nội dung thông báo không được để trống.")
                .MaximumLength(1000).WithMessage("Nội dung thông báo không được vượt quá 1000 ký tự.");
        }
    }
}
