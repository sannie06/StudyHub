using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class NhomHocTapRepository : GenericRepository<NhomHocTap>, INhomHocTapRepository
    {
        public NhomHocTapRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
