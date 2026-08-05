using FluentValidation;
using StudyHub.Application.DTOs.Auth;

namespace StudyHub.Application.Validators.Auth
{
    public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
    {
        public VerifyOtpRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã OTP không được để trống.")
                .Length(6).WithMessage("Mã OTP phải có đúng 6 ký tự.");

            RuleFor(x => x.LoaiOTP)
                .NotEmpty().WithMessage("Loại OTP không được để trống.");
        }
    }
}
