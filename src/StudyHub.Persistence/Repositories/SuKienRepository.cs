using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class SuKienRepository : GenericRepository<SuKien>, ISuKienRepository
    {
        public SuKienRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
