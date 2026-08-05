using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(StudyHubDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(rt => rt.NguoiDung)
                .ThenInclude(u => u.VaiTro)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var tokens = await _dbSet
                .Where(rt => rt.MaNguoiDung == userId && !rt.DaThuHoi && !rt.DaSuDung)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.DaThuHoi = true;
                token.NgayCapNhat = DateTime.UtcNow;
            }
        }
    }
}
