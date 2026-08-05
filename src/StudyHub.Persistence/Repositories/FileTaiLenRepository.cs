using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Persistence.Repositories
{
    public class FileTaiLenRepository : GenericRepository<FileTaiLen>, IFileTaiLenRepository
    {
        public FileTaiLenRepository(StudyHubDbContext context) : base(context)
        {
        }
    }
}
