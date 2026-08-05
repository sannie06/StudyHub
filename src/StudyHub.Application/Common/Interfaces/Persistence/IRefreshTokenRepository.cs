using System.Threading.Tasks;
using StudyHub.Domain.Entities;

namespace StudyHub.Application.Common.Interfaces.Persistence
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeAllUserTokensAsync(int userId);
    }
}
