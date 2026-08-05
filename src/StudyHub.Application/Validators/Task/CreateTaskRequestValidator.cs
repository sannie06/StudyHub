using FluentValidation;
using StudyHub.Application.DTOs.Task;

namespace StudyHub.Application.Validators.Task
{
    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskRequestValidator()
        {
            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề công việc không được để trống.")
                .MaximumLength(200).WithMessage("Tiêu đề công việc không vượt quá 200 ký tự.");

            RuleFor(x => x.DoUuTien)
                .Must(x => x <= 3).WithMessage("Độ ưu tiên không hợp lệ (0: Thấp, 1: Trung bình, 2: Cao, 3: Khẩn cấp).");

            RuleFor(x => x.TrangThai)
                .Must(x => x <= 4).WithMessage("Trạng thái không hợp lệ (0: Chưa bắt đầu, 1: Đang thực hiện, 2: Tạm dừng, 3: Hoàn thành, 4: Quá hạn).");

            RuleFor(x => x.TiLeHoanThanh)
                .InclusiveBetween(0, 100).WithMessage("Tỷ lệ hoàn thành phải nằm trong khoảng từ 0 đến 100.");

            RuleFor(x => x)
                .Must(x => !x.NgayBatDau.HasValue || !x.HanHoanThanh.HasValue || x.HanHoanThanh.Value >= x.NgayBatDau.Value)
                .WithMessage("Hạn hoàn thành phải lớn hơn hoặc bằng ngày bắt đầu.");
        }
    }
}
