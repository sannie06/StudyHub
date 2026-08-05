using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class ThanhVienNhomRepository : GenericRepository<ThanhVienNhom>, IThanhVienNhomRepository
    {
        public ThanhVienNhomRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
