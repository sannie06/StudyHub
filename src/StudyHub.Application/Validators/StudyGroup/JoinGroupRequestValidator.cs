using FluentValidation;
using StudyHub.Application.DTOs.StudyGroup;

namespace StudyHub.Application.Validators.StudyGroup
{
    public class JoinGroupRequestValidator : AbstractValidator<JoinGroupRequest>
    {
        public JoinGroupRequestValidator()
        {
            RuleFor(x => x.MaThamGia)
                .NotEmpty().WithMessage("Mã tham gia nhóm không được để trống.");
        }
    }
}
