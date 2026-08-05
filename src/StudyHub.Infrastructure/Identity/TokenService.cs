using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Identity
{
    public class TokenService : ITokenService
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public TokenService(
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public string GenerateAccessToken(NguoiDung nguoiDung)
        {
            return _jwtTokenGenerator.GenerateToken(nguoiDung);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<(string NewAccessToken, string NewRefreshToken)> RotateTokenAsync(string oldRefreshToken, string ipAddress)
        {
            var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(oldRefreshToken);

            if (refreshTokenEntity == null)
            {
                throw new UnauthorizedException("Invalid refresh token.");
            }

            // Replay Attack Detection: If a refresh token is reused, revoke all tokens for this user
            if (refreshTokenEntity.DaSuDung || refreshTokenEntity.DaThuHoi)
            {
                await _refreshTokenRepository.RevokeAllUserTokensAsync(refreshTokenEntity.MaNguoiDung);
                await _refreshTokenRepository.SaveAsync();
                throw new UnauthorizedException("Breach detected. Refresh token reused. All sessions revoked.");
            }

            if (refreshTokenEntity.NgayHetHan < DateTime.UtcNow)
            {
                throw new UnauthorizedException("Refresh token expired.");
            }

            // Rotate token: mark old one as used
            refreshTokenEntity.DaSuDung = true;
            refreshTokenEntity.NgayCapNhat = DateTime.UtcNow;
            _refreshTokenRepository.Update(refreshTokenEntity);

            // Generate new token pair
            var newAccessToken = _jwtTokenGenerator.GenerateToken(refreshTokenEntity.NguoiDung);
            var newRefreshTokenString = GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                MaNguoiDung = refreshTokenEntity.MaNguoiDung,
                Token = newRefreshTokenString,
                NgayHetHan = DateTime.UtcNow.AddDays(30), // Configurable or hardcoded for safety
                DaSuDung = false,
                DaThuHoi = false,
                NgayTao = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            await _refreshTokenRepository.SaveAsync();

            return (newAccessToken, newRefreshTokenString);
        }

        public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (tokenEntity == null)
            {
                throw new NotFoundException("Refresh token was not found.");
            }

            if (!tokenEntity.DaThuHoi)
            {
                tokenEntity.DaThuHoi = true;
                tokenEntity.NgayCapNhat = DateTime.UtcNow;
                _refreshTokenRepository.Update(tokenEntity);
                await _refreshTokenRepository.SaveAsync();
            }
        }
    }
}
