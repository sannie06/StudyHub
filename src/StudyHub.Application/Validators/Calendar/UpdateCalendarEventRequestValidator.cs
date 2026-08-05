using FluentValidation;
using StudyHub.Application.DTOs.Calendar;

namespace StudyHub.Application.Validators.Calendar
{
    public class UpdateCalendarEventRequestValidator : AbstractValidator<UpdateCalendarEventRequest>
    {
        public UpdateCalendarEventRequestValidator()
        {
            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề sự kiện không được để trống.")
                .MaximumLength(200).WithMessage("Tiêu đề sự kiện không được vượt quá 200 ký tự.");

            RuleFor(x => x.ThoiGianKetThuc)
                .GreaterThanOrEqualTo(x => x.ThoiGianBatDau).WithMessage("Thời gian kết thúc phải lớn hơn hoặc bằng thời gian bắt đầu.");
        }
    }
}
