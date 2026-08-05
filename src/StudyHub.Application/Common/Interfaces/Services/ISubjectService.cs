using System.Collections.Generic;
using System.Threading.Tasks;
using StudyHub.Application.DTOs.Subject;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface ISubjectService
    {
        Task<List<SubjectDto>> GetSubjectsAsync(int userId);
        Task<SubjectDto> GetSubjectByIdAsync(int id, int userId);
        Task<SubjectDto> CreateSubjectAsync(CreateSubjectRequest request);
        Task<SubjectDto> UpdateSubjectAsync(int id, UpdateSubjectRequest request);
        Task DeleteSubjectAsync(int id);
    }
}
