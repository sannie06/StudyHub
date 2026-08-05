using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class TaiLieuRepository : GenericRepository<TaiLieu>, ITaiLieuRepository
    {
        public TaiLieuRepository(StudyHubDbContext context) : base(context)
        {
        }
    }
}
