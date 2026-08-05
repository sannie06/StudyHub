using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class LoaiThongBaoRepository : GenericRepository<LoaiThongBao>, ILoaiThongBaoRepository
    {
        public LoaiThongBaoRepository(StudyHubDbContext dbContext) : base(dbContext)
        {
        }
    }
}
