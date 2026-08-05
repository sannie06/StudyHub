using FluentValidation;
using StudyHub.Application.DTOs.Chat;

namespace StudyHub.Application.Validators.Chat
{
    public class SendChatMessageRequestValidator : AbstractValidator<SendChatMessageRequest>
    {
        public SendChatMessageRequestValidator()
        {
            RuleFor(x => x.MaNhom)
                .GreaterThan(0).WithMessage("Mã nhóm học tập không hợp lệ.");

            RuleFor(x => x.NoiDung)
                .NotEmpty().WithMessage("Nội dung tin nhắn không được để trống.")
                .MaximumLength(2000).WithMessage("Nội dung tin nhắn không được vượt quá 2000 ký tự.");
        }
    }
}
