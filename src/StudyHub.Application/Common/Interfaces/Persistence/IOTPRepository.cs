using System.Threading.Tasks;
using StudyHub.Domain.Entities;

namespace StudyHub.Application.Common.Interfaces.Persistence
{
    public interface IOTPRepository : IGenericRepository<OTP>
    {
        Task<OTP?> GetLatestOtpAsync(string email, string loaiOTP);
    }
}
