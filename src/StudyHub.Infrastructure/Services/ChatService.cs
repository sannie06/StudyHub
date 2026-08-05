using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Chat;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly ITinNhanRepository _chatRepository;
        private readonly IThanhVienNhomRepository _memberRepository;
        private readonly INhomHocTapRepository _groupRepository;
        private readonly IGenericRepository<NguoiDung> _userRepository;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            ITinNhanRepository chatRepository,
            IThanhVienNhomRepository memberRepository,
            INhomHocTapRepository groupRepository,
            IGenericRepository<NguoiDung> userRepository,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _memberRepository = memberRepository;
            _groupRepository = groupRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        private async Task ValidateMemberAsync(int groupId, int userId)
        {
            var isMember = await _memberRepository.GetQueryable().AsNoTracking()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);

            if (!isMember)
            {
                throw new UnauthorizedException("Bạn không phải thành viên của nhóm học tập này.");
            }
        }

        public async Task<IEnumerable<TinNhanDto>> GetGroupMessagesAsync(int groupId, int userId, int page = 1, int pageSize = 50)
        {
            await ValidateMemberAsync(groupId, userId);

            var query = _chatRepository.GetQueryable()
                .AsNoTracking()
                .Include(m => m.NguoiGui)
                .Where(m => m.MaNhom == groupId && !m.DaXoa)
                .OrderByDescending(m => m.NgayGui)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var messages = await query.ToListAsync();
            messages.Reverse(); // Chronological order for chat display

            return messages.Select(m => new TinNhanDto
            {
                MaTinNhan = m.MaTinNhan,
                MaNhom = m.MaNhom,
                MaNguoiGui = m.MaNguoiGui,
                TenNguoiGui = m.NguoiGui?.HoTen ?? string.Empty,
                AvatarNguoiGui = m.NguoiGui?.AnhDaiDien,
                NoiDung = m.NoiDung,
                LoaiTinNhan = m.LoaiTinNhan,
                DaChinhSua = m.DaChinhSua,
                NgayGui = m.NgayGui,
                IsMine = m.MaNguoiGui == userId
            });
        }

        public async Task<TinNhanDto> SendMessageAsync(int userId, SendChatMessageRequest request)
        {
            await ValidateMemberAsync(request.MaNhom, userId);

            var sender = await _userRepository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(u => u.MaNguoiDung == userId);
            if (sender == null)
            {
                throw new NotFoundException("Người gửi không tồn tại.");
            }

            var message = new TinNhan
            {
                MaNhom = request.MaNhom,
                MaNguoiGui = userId,
                NoiDung = request.NoiDung.Trim(),
                LoaiTinNhan = request.LoaiTinNhan,
                NgayGui = DateTime.Now,
                DaXoa = false
            };

            await _chatRepository.AddAsync(message);
            await _chatRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã gửi tin nhắn đến nhóm {GroupId}", userId, request.MaNhom);

            return new TinNhanDto
            {
                MaTinNhan = message.MaTinNhan,
                MaNhom = message.MaNhom,
                MaNguoiGui = message.MaNguoiGui,
                TenNguoiGui = sender.HoTen,
                AvatarNguoiGui = sender.AnhDaiDien,
                NoiDung = message.NoiDung,
                LoaiTinNhan = message.LoaiTinNhan,
                DaChinhSua = false,
                NgayGui = message.NgayGui,
                IsMine = true
            };
        }

        public async Task DeleteMessageAsync(int messageId, int userId)
        {
            var message = await _chatRepository.GetQueryable().FirstOrDefaultAsync(m => m.MaTinNhan == messageId && !m.DaXoa);
            if (message == null)
            {
                throw new NotFoundException("Tin nhắn không tồn tại hoặc đã bị xóa.");
            }

            if (message.MaNguoiGui != userId)
            {
                var isOwner = await _groupRepository.GetQueryable().AsNoTracking()
                    .AnyAsync(g => g.MaNhom == message.MaNhom && g.MaNguoiTao == userId);

                if (!isOwner)
                {
                    throw new UnauthorizedException("Bạn không có quyền xóa tin nhắn này.");
                }
            }

            message.DaXoa = true;
            _chatRepository.Update(message);
            await _chatRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã xóa tin nhắn {MessageId}", userId, messageId);
        }
    }
}
