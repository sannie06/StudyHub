using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class LichHocRepository : GenericRepository<LichHoc>, ILichHocRepository
    {
        public LichHocRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
