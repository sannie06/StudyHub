using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Notification;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IThongBaoRepository _notificationRepository;
        private readonly ILoaiThongBaoRepository _typeRepository;
        private readonly IGenericRepository<NguoiDung> _userRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public NotificationService(
            IThongBaoRepository notificationRepository,
            ILoaiThongBaoRepository typeRepository,
            IGenericRepository<NguoiDung> userRepository,
            ILogger<NotificationService> logger,
            IServiceProvider serviceProvider)
        {
            _notificationRepository = notificationRepository;
            _typeRepository = typeRepository;
            _userRepository = userRepository;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<ThongBaoDto>> GetMyNotificationsAsync(int userId, bool? unreadOnly = null, int page = 1, int pageSize = 20)
        {
            var query = _notificationRepository.GetQueryable()
                .AsNoTracking()
                .Include(n => n.LoaiThongBao)
                .Where(n => n.MaNguoiDung == userId && !n.DaXoa);

            if (unreadOnly.HasValue && unreadOnly.Value)
            {
                query = query.Where(n => !n.DaDoc);
            }

            var list = await query
                .OrderByDescending(n => n.NgayGui)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return list.Select(n => new ThongBaoDto
            {
                MaThongBao = n.MaThongBao,
                MaNguoiDung = n.MaNguoiDung,
                MaLoaiThongBao = n.MaLoaiThongBao,
                TenLoaiThongBao = n.LoaiThongBao?.TenLoai ?? string.Empty,
                Icon = n.LoaiThongBao?.Icon ?? "pi-bell",
                MauSac = n.LoaiThongBao?.MauSac ?? "blue",
                TieuDe = n.TieuDe,
                NoiDung = n.NoiDung,
                DuongDan = n.DuongDan ?? string.Empty,
                DaDoc = n.DaDoc,
                MucDo = n.MucDo,
                NgayGui = n.NgayGui,
                NgayDoc = n.NgayDoc
            });
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.GetQueryable()
                .AsNoTracking()
                .CountAsync(n => n.MaNguoiDung == userId && !n.DaDoc && !n.DaXoa);
        }

        public async Task<ThongBaoDto> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetQueryable()
                .Include(n => n.LoaiThongBao)
                .FirstOrDefaultAsync(n => n.MaThongBao == notificationId && n.MaNguoiDung == userId && !n.DaXoa);

            if (notification == null)
            {
                throw new NotFoundException("Thông báo không tồn tại.");
            }

            if (!notification.DaDoc)
            {
                notification.DaDoc = true;
                notification.NgayDoc = DateTime.Now;
                _notificationRepository.Update(notification);
                await _notificationRepository.SaveAsync();
            }

            return new ThongBaoDto
            {
                MaThongBao = notification.MaThongBao,
                MaNguoiDung = notification.MaNguoiDung,
                MaLoaiThongBao = notification.MaLoaiThongBao,
                TenLoaiThongBao = notification.LoaiThongBao?.TenLoai ?? string.Empty,
                Icon = notification.LoaiThongBao?.Icon ?? "pi-bell",
                MauSac = notification.LoaiThongBao?.MauSac ?? "blue",
                TieuDe = notification.TieuDe,
                NoiDung = notification.NoiDung,
                DuongDan = notification.DuongDan ?? string.Empty,
                DaDoc = notification.DaDoc,
                MucDo = notification.MucDo,
                NgayGui = notification.NgayGui,
                NgayDoc = notification.NgayDoc
            };
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadList = await _notificationRepository.GetQueryable()
                .Where(n => n.MaNguoiDung == userId && !n.DaDoc && !n.DaXoa)
                .ToListAsync();

            if (unreadList.Any())
            {
                var now = DateTime.Now;
                foreach (var item in unreadList)
                {
                    item.DaDoc = true;
                    item.NgayDoc = now;
                    _notificationRepository.Update(item);
                }
                await _notificationRepository.SaveAsync();
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetQueryable()
                .FirstOrDefaultAsync(n => n.MaThongBao == notificationId && n.MaNguoiDung == userId && !n.DaXoa);

            if (notification == null)
            {
                throw new NotFoundException("Thông báo không tồn tại.");
            }

            notification.DaXoa = true;
            _notificationRepository.Update(notification);
            await _notificationRepository.SaveAsync();
        }

        public async Task<ThongBaoDto> CreateNotificationAsync(CreateNotificationRequest request)
        {
            var userExists = await _userRepository.GetQueryable().AsNoTracking().AnyAsync(u => u.MaNguoiDung == request.MaNguoiDung);
            if (!userExists)
            {
                throw new NotFoundException("Người dùng nhận thông báo không tồn tại.");
            }

            var type = await _typeRepository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(t => t.MaLoaiThongBao == request.MaLoaiThongBao);

            var notification = new ThongBao
            {
                MaNguoiDung = request.MaNguoiDung,
                MaLoaiThongBao = request.MaLoaiThongBao,
                TieuDe = request.TieuDe.Trim(),
                NoiDung = request.NoiDung.Trim(),
                DuongDan = request.DuongDan,
                DaDoc = false,
                MucDo = request.MucDo,
                NgayGui = DateTime.Now,
                DaXoa = false
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveAsync();

            var dto = new ThongBaoDto
            {
                MaThongBao = notification.MaThongBao,
                MaNguoiDung = notification.MaNguoiDung,
                MaLoaiThongBao = notification.MaLoaiThongBao,
                TenLoaiThongBao = type?.TenLoai ?? "Thông báo",
                Icon = type?.Icon ?? "pi-bell",
                MauSac = type?.MauSac ?? "blue",
                TieuDe = notification.TieuDe,
                NoiDung = notification.NoiDung,
                DuongDan = notification.DuongDan ?? string.Empty,
                DaDoc = false,
                MucDo = notification.MucDo,
                NgayGui = notification.NgayGui
            };

            // Push SignalR notification via INotificationRealtimeService
            try
            {
                var realtimeService = _serviceProvider.GetService(typeof(INotificationRealtimeService)) as INotificationRealtimeService;
                if (realtimeService != null)
                {
                    await realtimeService.SendNotificationToUserAsync(request.MaNguoiDung, dto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể phát SignalR push notification cho người dùng {UserId}", request.MaNguoiDung);
            }

            return dto;
        }
    }
}
