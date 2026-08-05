using FluentValidation;
using StudyHub.Application.DTOs.StudyGroup;

namespace StudyHub.Application.Validators.StudyGroup
{
    public class UpdateStudyGroupRequestValidator : AbstractValidator<UpdateStudyGroupRequest>
    {
        public UpdateStudyGroupRequestValidator()
        {
            RuleFor(x => x.TenNhom)
                .NotEmpty().WithMessage("Tên nhóm học tập không được để trống.")
                .MaximumLength(100).WithMessage("Tên nhóm không được vượt quá 100 ký tự.");

            RuleFor(x => x.SoLuongToiDa)
                .InclusiveBetween(2, 100).WithMessage("Số lượng thành viên tối đa phải từ 2 đến 100 người.");
        }
    }
}
