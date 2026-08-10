using System;
using System.Text.RegularExpressions;
using FluentValidation;
using StudyHub.Application.DTOs.Auth;

namespace StudyHub.Application.Validators.Auth
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .Matches(EmailRegex).WithMessage("Email không đúng định dạng chuẩn (ví dụ: yourname@gmail.com).")
                .Must(NotBeTypoDomain).WithMessage("Tên miền email không hợp lệ hoặc bị lỗi chính tả (ví dụ: @gmail.com hoặc @*.edu.vn).");
        }

        private bool NotBeTypoDomain(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var parts = email.Split('@');
            if (parts.Length != 2) return false;
            var domain = parts[1].ToLower().Trim();

            var invalidDomains = new[] { "gmsha.com", "glioail.com", "gmai.com", "gamil.com", "yaho.com", "hotmial.com", "outlok.com" };
            foreach (var inv in invalidDomains)
            {
                if (domain == inv) return false;
            }

            return domain.Contains('.') && !domain.StartsWith(".") && !domain.EndsWith(".");
        }
    }
}
