using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Chat;
using StudyHub.Domain.Entities;
using StudyHub.Persistence;

namespace StudyHub.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly ITinNhanRepository _chatRepository;
        private readonly IThanhVienNhomRepository _memberRepository;
        private readonly INhomHocTapRepository _groupRepository;
        private readonly IGenericRepository<NguoiDung> _userRepository;
        private readonly StudyHubDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ChatService> _logger;

        private static readonly ConcurrentDictionary<int, string> PinnedAnnouncements = new();

        public ChatService(
            ITinNhanRepository chatRepository,
            IThanhVienNhomRepository memberRepository,
            INhomHocTapRepository groupRepository,
            IGenericRepository<NguoiDung> userRepository,
            StudyHubDbContext dbContext,
            IWebHostEnvironment environment,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _memberRepository = memberRepository;
            _groupRepository = groupRepository;
            _userRepository = userRepository;
            _dbContext = dbContext;
            _environment = environment;
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
                .Include(m => m.TepDinhKemTinNhan)
                    .ThenInclude(t => t.FileTaiLen)
                .Where(m => m.MaNhom == groupId && !m.DaXoa)
                .OrderByDescending(m => m.NgayGui)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var messages = await query.ToListAsync();
            messages.Reverse(); // Chronological order for chat display

            var allFiles = await _dbContext.FileTaiLen.AsNoTracking().ToListAsync();

            return messages.Select(m =>
            {
                var att = m.TepDinhKemTinNhan?.FirstOrDefault()?.FileTaiLen;
                var fileNameLower = m.NoiDung?.Trim().ToLower() ?? string.Empty;
                var fileExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".zip", ".rar", ".txt" };
                var hasFileExtension = fileExtensions.Any(ext => fileNameLower.EndsWith(ext));

                if (att == null && (m.LoaiTinNhan > 0 || hasFileExtension))
                {
                    // Fallback to match from FileTaiLen by TenGoc or TenLuu
                    var matchedFile = allFiles.FirstOrDefault(f => f.TenGoc == m.NoiDung || f.TenLuu == m.NoiDung);
                    if (matchedFile != null)
                    {
                        att = matchedFile;
                    }
                }

                TepDinhKemChatDto? chatAttachment = null;
                if (att != null)
                {
                    chatAttachment = new TepDinhKemChatDto
                    {
                        MaFile = att.MaFile,
                        TenFile = att.TenGoc,
                        DuongDan = att.DuongDan,
                        DungLuong = att.DungLuong,
                        DinhDang = att.Extension
                    };
                }
                else if (hasFileExtension)
                {
                    var ext = Path.GetExtension(m.NoiDung);
                    chatAttachment = new TepDinhKemChatDto
                    {
                        MaFile = 0,
                        TenFile = m.NoiDung,
                        DuongDan = $"/uploads/chat/{m.NoiDung}",
                        DungLuong = 70144,
                        DinhDang = ext
                    };
                }

                return new TinNhanDto
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
                    IsMine = m.MaNguoiGui == userId,
                    Attachment = chatAttachment
                };
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

        public async Task<TinNhanDto> SendFileMessageAsync(int userId, int groupId, IFormFile file, string? content)
        {
            await ValidateMemberAsync(groupId, userId);

            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Tập tin tải lên không hợp lệ.");
            }

            var sender = await _userRepository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(u => u.MaNguoiDung == userId);
            if (sender == null)
            {
                throw new NotFoundException("Người gửi không tồn tại.");
            }

            // Save file to wwwroot/uploads/chat folder
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "src", "StudyHub.Web", "wwwroot");
                if (!Directory.Exists(webRoot))
                {
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
            }

            var uploadsFolder = Path.Combine(webRoot, "uploads", "chat");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            var uniqueFileName = $"chat_{groupId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create FileTaiLen entity
            var fileRecord = new FileTaiLen
            {
                MaNguoiDung = userId,
                TenGoc = file.FileName,
                TenLuu = uniqueFileName,
                DuongDan = $"/uploads/chat/{uniqueFileName}",
                LoaiFile = file.ContentType ?? "application/octet-stream",
                DungLuong = file.Length,
                Extension = extension,
                NgayTaiLen = DateTime.Now,
                DaXoa = false
            };

            await _dbContext.FileTaiLen.AddAsync(fileRecord);
            await _dbContext.SaveChangesAsync();

            var isImage = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(extension);

            // Create TinNhan entity
            var message = new TinNhan
            {
                MaNhom = groupId,
                MaNguoiGui = userId,
                NoiDung = string.IsNullOrWhiteSpace(content) ? file.FileName : content.Trim(),
                LoaiTinNhan = (byte)(isImage ? 1 : 2),
                NgayGui = DateTime.Now,
                DaXoa = false
            };

            await _chatRepository.AddAsync(message);
            await _chatRepository.SaveAsync();

            // Link Attachment
            var attachment = new TepDinhKemTinNhan
            {
                MaTinNhan = message.MaTinNhan,
                MaFile = fileRecord.MaFile
            };
            await _dbContext.TepDinhKemTinNhan.AddAsync(attachment);

            // Automatically sync file to Group Documents (Tab Tài liệu) under group folder
            try
            {
                var group = await _groupRepository.GetQueryable().FirstOrDefaultAsync(g => g.MaNhom == groupId);
                var folderName = group?.TenNhom ?? "Tài liệu nhóm";

                var folder = await _dbContext.ThuMucTaiLieu
                    .FirstOrDefaultAsync(f => f.MaNhom == groupId && f.TenThuMuc == folderName);

                if (folder == null)
                {
                    folder = new ThuMucTaiLieu
                    {
                        MaNhom = groupId,
                        MaNguoiTao = userId,
                        TenThuMuc = folderName,
                        MoTa = $"Thư mục tài liệu của nhóm {folderName}"
                    };
                    await _dbContext.ThuMucTaiLieu.AddAsync(folder);
                    await _dbContext.SaveChangesAsync();
                }

                var document = new TaiLieu
                {
                    MaNhom = groupId,
                    MaNguoiTaiLen = userId,
                    MaFile = fileRecord.MaFile,
                    MaThuMuc = folder.MaThuMuc,
                    TieuDe = file.FileName,
                    MoTa = string.IsNullOrWhiteSpace(content) ? $"Tài liệu gửi qua Chat nhóm lúc {DateTime.Now:dd/MM/yyyy HH:mm}" : content.Trim(),
                    NgayTaiLen = DateTime.Now,
                    DaXoa = false,
                    LuotTai = 0
                };
                await _dbContext.TaiLieu.AddAsync(document);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể tự động đồng bộ tệp sang bảng Tài liệu nhóm");
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Người dùng {UserId} đã gửi tệp {FileName} vào nhóm {GroupId}", userId, file.FileName, groupId);

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
                IsMine = true,
                Attachment = new TepDinhKemChatDto
                {
                    MaFile = fileRecord.MaFile,
                    TenFile = fileRecord.TenGoc,
                    DuongDan = fileRecord.DuongDan,
                    DungLuong = fileRecord.DungLuong,
                    DinhDang = fileRecord.Extension
                }
            };
        }

        public async Task<string> GetPinnedAnnouncementAsync(int groupId, int userId)
        {
            await ValidateMemberAsync(groupId, userId);

            if (PinnedAnnouncements.TryGetValue(groupId, out var pin) && !string.IsNullOrWhiteSpace(pin))
            {
                return pin;
            }

            var group = await _groupRepository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(g => g.MaNhom == groupId);
            return group?.MoTa ?? "Chào mừng các thành viên đến với nhóm học tập! Hãy cùng nhau thảo luận và hoàn thành tốt nhiệm vụ nhé. 🚀";
        }

        public async Task UpdatePinnedAnnouncementAsync(int groupId, int userId, string announcement)
        {
            await ValidateMemberAsync(groupId, userId);

            var pinText = (announcement ?? string.Empty).Trim();
            PinnedAnnouncements[groupId] = pinText;

            var group = await _groupRepository.GetQueryable().FirstOrDefaultAsync(g => g.MaNhom == groupId);
            if (group != null)
            {
                group.MoTa = pinText;
                _groupRepository.Update(group);
                await _groupRepository.SaveAsync();
            }

            _logger.LogInformation("Người dùng {UserId} đã cập nhật thông báo ghim cho nhóm {GroupId}", userId, groupId);
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
