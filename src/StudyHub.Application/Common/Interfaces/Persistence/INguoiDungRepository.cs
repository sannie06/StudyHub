using System.Threading.Tasks;
using StudyHub.Domain.Entities;

namespace StudyHub.Application.Common.Interfaces.Persistence
{
    public interface INguoiDungRepository : IGenericRepository<NguoiDung>
    {
        Task<NguoiDung?> GetByEmailAsync(string email);
        Task<NguoiDung?> GetWithRolesAsync(string email);
        Task<bool> IsEmailUniqueAsync(string email);
    }
}
