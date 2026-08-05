using StudyHub.Domain.Entities;

namespace StudyHub.Application.Common.Interfaces.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(NguoiDung nguoiDung);
    }
}
