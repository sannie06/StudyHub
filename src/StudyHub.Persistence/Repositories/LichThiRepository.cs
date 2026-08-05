using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class LichThiRepository : GenericRepository<LichThi>, ILichThiRepository
    {
        public LichThiRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
