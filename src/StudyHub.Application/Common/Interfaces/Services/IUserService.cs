using System.Threading.Tasks;
using StudyHub.Application.DTOs.User;

namespace StudyHub.Application.Common.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<string> UpdateAvatarAsync(int userId, string avatarUrl);
        Task<UserStatsDto> GetStatisticsAsync(int userId);
    }
}
