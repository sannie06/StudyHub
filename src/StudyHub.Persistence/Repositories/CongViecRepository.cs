using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class CongViecRepository : GenericRepository<CongViec>, ICongViecRepository
    {
        public CongViecRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
