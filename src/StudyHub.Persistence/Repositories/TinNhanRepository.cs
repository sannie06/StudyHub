using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class TinNhanRepository : GenericRepository<TinNhan>, ITinNhanRepository
    {
        public TinNhanRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
