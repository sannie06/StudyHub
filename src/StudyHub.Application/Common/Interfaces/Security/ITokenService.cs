using System.Threading.Tasks;
using StudyHub.Domain.Entities;

namespace StudyHub.Application.Common.Interfaces.Security
{
    public interface ITokenService
    {
        /// <summary>
        /// Generates a standard cryptographically signed JWT Access Token
        /// </summary>
        string GenerateAccessToken(NguoiDung nguoiDung);

        /// <summary>
        /// Generates a secure, unique Refresh Token string
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Handles Refresh Token Rotation (RTR). 
        /// Validates the old token, revokes it, and issues a new pair of Access & Refresh tokens.
        /// Protects against token reuse hijacking.
        /// </summary>
        Task<(string NewAccessToken, string NewRefreshToken)> RotateTokenAsync(string oldRefreshToken, string ipAddress);

        /// <summary>
        /// Revokes a specific Refresh Token.
        /// Used during logout or session termination.
        /// </summary>
        Task RevokeTokenAsync(string refreshToken, string ipAddress);
    }
}
