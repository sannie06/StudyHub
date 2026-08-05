using System;
using System.Linq;
using System.Threading.Tasks;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Security;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.User;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly INguoiDungRepository _nguoiDungRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IGenericRepository<ThongKeHocTap> _statsRepository;

        public UserService(
            INguoiDungRepository nguoiDungRepository,
            IPasswordHasher passwordHasher,
            IGenericRepository<ThongKeHocTap> statsRepository)
        {
            _nguoiDungRepository = nguoiDungRepository;
            _passwordHasher = passwordHasher;
            _statsRepository = statsRepository;
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _nguoiDungRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            // Load roles if available (we can query again with roles if needed)
            var userWithRole = await _nguoiDungRepository.GetWithRolesAsync(user.Email);
            
            return MapToProfileDto(userWithRole ?? user);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _nguoiDungRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            user.HoTen = request.HoTen;
            user.SoDienThoai = request.SoDienThoai;
            user.NgaySinh = request.NgaySinh;
            user.GioiTinh = request.GioiTinh;
            user.DiaChi = request.DiaChi;
            user.NgayCapNhat = DateTime.UtcNow;

            _nguoiDungRepository.Update(user);
            await _nguoiDungRepository.SaveAsync();

            var userWithRole = await _nguoiDungRepository.GetWithRolesAsync(user.Email);

            return MapToProfileDto(userWithRole ?? user);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _nguoiDungRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            if (!_passwordHasher.VerifyPassword(request.OldPassword, user.MatKhauHash))
            {
                throw new BadRequestException("Mật khẩu cũ không chính xác.");
            }

            user.MatKhauHash = _passwordHasher.HashPassword(request.NewPassword);
            user.NgayCapNhat = DateTime.UtcNow;

            _nguoiDungRepository.Update(user);
            await _nguoiDungRepository.SaveAsync();
        }

        public async Task<string> UpdateAvatarAsync(int userId, string avatarUrl)
        {
            var user = await _nguoiDungRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            user.AnhDaiDien = avatarUrl;
            user.NgayCapNhat = DateTime.UtcNow;

            _nguoiDungRepository.Update(user);
            await _nguoiDungRepository.SaveAsync();

            return avatarUrl;
        }

        public async Task<UserStatsDto> GetStatisticsAsync(int userId)
        {
            var statsList = await _statsRepository.FindAsync(s => s.MaNguoiDung == userId);
            var latestStats = statsList.OrderByDescending(s => s.NgayThongKe).FirstOrDefault();

            if (latestStats == null)
            {
                // Return default zeroed stats if none exist yet
                return new UserStatsDto
                {
                    TongCongViec = 0,
                    CongViecHoanThanh = 0,
                    CongViecQuaHan = 0,
                    TongPomodoro = 0,
                    TongPhutHoc = 0,
                    SoNgayHocLienTiep = 0,
                    TyLeHoanThanh = 0,
                    DiemNangSuat = 0
                };
            }

            return new UserStatsDto
            {
                TongCongViec = latestStats.TongCongViec,
                CongViecHoanThanh = latestStats.CongViecHoanThanh,
                CongViecQuaHan = latestStats.CongViecQuaHan,
                TongPomodoro = latestStats.TongPomodoro,
                TongPhutHoc = latestStats.TongPhutHoc,
                SoNgayHocLienTiep = latestStats.SoNgayHocLienTiep,
                TyLeHoanThanh = latestStats.TyLeHoanThanh,
                DiemNangSuat = latestStats.DiemNangSuat
            };
        }

        private UserProfileDto MapToProfileDto(NguoiDung user)
        {
            return new UserProfileDto
            {
                MaNguoiDung = user.MaNguoiDung,
                Email = user.Email,
                HoTen = user.HoTen,
                SoDienThoai = user.SoDienThoai,
                NgaySinh = user.NgaySinh,
                GioiTinh = user.GioiTinh,
                DiaChi = user.DiaChi,
                AnhDaiDien = user.AnhDaiDien,
                VaiTro = user.VaiTro?.TenVaiTro ?? "Student"
            };
        }
    }
}
