using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class OTPRepository : GenericRepository<OTP>, IOTPRepository
    {
        public OTPRepository(StudyHubDbContext context) : base(context)
        {
        }

        public async Task<OTP?> GetLatestOtpAsync(string email, string loaiOTP)
        {
            return await _dbSet
                .Where(o => o.Email == email && o.LoaiOTP == loaiOTP && !o.DaSuDung)
                .OrderByDescending(o => o.NgayTao)
                .FirstOrDefaultAsync();
        }
    }
}
