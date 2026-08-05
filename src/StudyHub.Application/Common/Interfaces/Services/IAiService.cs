using System.Threading.Tasks;
using StudyHub.Application.DTOs.Ai;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IAiService
    {
        Task<AiChatResponse> ChatAsync(int userId, AiChatRequest request);
        Task<StudyPlanResponse> GenerateStudyPlanAsync(int userId, StudyPlanRequest request);
        Task<string> AnalyzeWorkloadAsync(int userId);
        Task<string> GetStudyAdviceAsync(int userId);
    }
}
