using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class CongViecNhomRepository : GenericRepository<CongViecNhom>, ICongViecNhomRepository
    {
        public CongViecNhomRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
