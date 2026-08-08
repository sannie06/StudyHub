using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class NguoiDungRepository : GenericRepository<NguoiDung>, INguoiDungRepository
    {
        public NguoiDungRepository(StudyHubDbContext context) : base(context)
        {
        }

        public async Task<NguoiDung?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.DaXoa);
        }

        public async Task<NguoiDung?> GetWithRolesAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _dbSet
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.DaXoa);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _dbSet.AnyAsync(u => u.Email == email);
        }
    }
}
