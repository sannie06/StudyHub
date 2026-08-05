using FluentValidation;
using StudyHub.Application.DTOs.Auth;

namespace StudyHub.Application.Validators.Auth
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");

            RuleFor(x => x.MatKhau)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.");
        }
    }
}
