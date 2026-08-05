using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.TaiLieu;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class TaiLieuService : ITaiLieuService
    {
        private readonly ITaiLieuRepository _taiLieuRepository;
        private readonly IFileTaiLenRepository _fileRepository;
        private readonly IGenericRepository<ThanhVienNhom> _memberRepository;
        private readonly IGenericRepository<NhomHocTap> _groupRepository;
        private readonly IGenericRepository<NguoiDung> _userRepository;
        private readonly ILogger<TaiLieuService> _logger;

        public TaiLieuService(
            ITaiLieuRepository taiLieuRepository,
            IFileTaiLenRepository fileRepository,
            IGenericRepository<ThanhVienNhom> memberRepository,
            IGenericRepository<NhomHocTap> groupRepository,
            IGenericRepository<NguoiDung> userRepository,
            ILogger<TaiLieuService> logger)
        {
            _taiLieuRepository = taiLieuRepository;
            _fileRepository = fileRepository;
            _memberRepository = memberRepository;
            _groupRepository = groupRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        private async Task ValidateMemberAsync(int userId, int maNhom)
        {
            var groupExists = await _groupRepository.GetQueryable().AsNoTracking().AnyAsync(g => g.MaNhom == maNhom);
            if (!groupExists)
            {
                throw new NotFoundException("Nhóm học tập không tồn tại.");
            }

            var isMember = await _memberRepository.GetQueryable().AsNoTracking()
                .AnyAsync(m => m.MaNhom == maNhom && m.MaNguoiDung == userId);
            if (!isMember)
            {
                throw new UnauthorizedException("Bạn không có quyền truy cập tài liệu của nhóm học tập này.");
            }
        }

        public async Task<IEnumerable<TaiLieuDto>> GetDocumentsAsync(int userId, int maNhom, string? search)
        {
            await ValidateMemberAsync(userId, maNhom);

            var query = _taiLieuRepository.GetQueryable()
                .AsNoTracking()
                .Where(t => t.MaNhom == maNhom && !t.DaXoa);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(t => t.TieuDe.ToLower().Contains(searchLower) || 
                                         (t.MoTa != null && t.MoTa.ToLower().Contains(searchLower)));
            }

            return await query.Select(t => new TaiLieuDto
            {
                MaTaiLieu = t.MaTaiLieu,
                MaNhom = t.MaNhom,
                MaNguoiTaiLen = t.MaNguoiTaiLen,
                TenNguoiTaiLen = t.NguoiTaiLen != null ? t.NguoiTaiLen.HoTen : string.Empty,
                TieuDe = t.TieuDe,
                MoTa = t.MoTa ?? string.Empty,
                LuotTai = t.LuotTai,
                NgayTaiLen = t.NgayTaiLen,
                NgayCapNhat = t.NgayCapNhat,
                MaFile = t.MaFile,
                TenGoc = t.FileTaiLen != null ? t.FileTaiLen.TenGoc : string.Empty,
                LoaiFile = t.FileTaiLen != null ? t.FileTaiLen.LoaiFile : string.Empty,
                DungLuong = t.FileTaiLen != null ? t.FileTaiLen.DungLuong : 0,
                Extension = t.FileTaiLen != null ? t.FileTaiLen.Extension : string.Empty
            }).ToListAsync();
        }

        public async Task<TaiLieuDto> GetDocumentByIdAsync(int id, int userId)
        {
            var t = await _taiLieuRepository.GetQueryable()
                .AsNoTracking()
                .Include(x => x.NguoiTaiLen)
                .Include(x => x.FileTaiLen)
                .FirstOrDefaultAsync(x => x.MaTaiLieu == id && !x.DaXoa);

            if (t == null)
            {
                throw new NotFoundException("Tài liệu không tồn tại.");
            }

            await ValidateMemberAsync(userId, t.MaNhom);

            return new TaiLieuDto
            {
                MaTaiLieu = t.MaTaiLieu,
                MaNhom = t.MaNhom,
                MaNguoiTaiLen = t.MaNguoiTaiLen,
                TenNguoiTaiLen = t.NguoiTaiLen?.HoTen ?? string.Empty,
                TieuDe = t.TieuDe,
                MoTa = t.MoTa ?? string.Empty,
                LuotTai = t.LuotTai,
                NgayTaiLen = t.NgayTaiLen,
                NgayCapNhat = t.NgayCapNhat,
                MaFile = t.MaFile,
                TenGoc = t.FileTaiLen?.TenGoc ?? string.Empty,
                LoaiFile = t.FileTaiLen?.LoaiFile ?? string.Empty,
                DungLuong = t.FileTaiLen?.DungLuong ?? 0,
                Extension = t.FileTaiLen?.Extension ?? string.Empty
            };
        }

        public async Task<TaiLieuDto> UploadDocumentAsync(int userId, UploadDocumentRequest request)
        {
            await ValidateMemberAsync(userId, request.MaNhom);

            if (request.File == null || request.File.Length == 0)
            {
                throw new BadRequestException("Tập tin tải lên không hợp lệ.");
            }

            if (request.File.Length > 50 * 1024 * 1024)
            {
                throw new BadRequestException("Kích thước tệp tin không được vượt quá 50MB.");
            }

            var allowedExtensions = new[] { ".pdf", ".docx", ".pptx", ".xlsx", ".png", ".jpg", ".zip" };
            var extension = Path.GetExtension(request.File.FileName).ToLower();
            if (Array.IndexOf(allowedExtensions, extension) < 0)
            {
                throw new BadRequestException("Định dạng tệp không được hỗ trợ.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var fileRecord = new FileTaiLen
            {
                MaNguoiDung = userId,
                TenGoc = request.File.FileName,
                TenLuu = uniqueFileName,
                DuongDan = $"/uploads/documents/{uniqueFileName}",
                LoaiFile = request.File.ContentType,
                DungLuong = request.File.Length,
                Extension = extension,
                NgayTaiLen = DateTime.Now
            };

            await _fileRepository.AddAsync(fileRecord);
            await _fileRepository.SaveAsync();

            var document = new TaiLieu
            {
                MaNhom = request.MaNhom,
                MaNguoiTaiLen = userId,
                MaFile = fileRecord.MaFile,
                TieuDe = request.TieuDe,
                MoTa = request.MoTa,
                LuotTai = 0,
                NgayTaiLen = DateTime.Now,
                DaXoa = false
            };

            await _taiLieuRepository.AddAsync(document);
            await _taiLieuRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã tải lên tài liệu {DocumentId}: {TieuDe}", userId, document.MaTaiLieu, document.TieuDe);

            var uploaderName = await _userRepository.GetQueryable()
                .AsNoTracking()
                .Where(u => u.MaNguoiDung == userId)
                .Select(u => u.HoTen)
                .FirstOrDefaultAsync() ?? string.Empty;

            return new TaiLieuDto
            {
                MaTaiLieu = document.MaTaiLieu,
                MaNhom = document.MaNhom,
                MaNguoiTaiLen = document.MaNguoiTaiLen,
                TenNguoiTaiLen = uploaderName,
                TieuDe = document.TieuDe,
                MoTa = document.MoTa ?? string.Empty,
                LuotTai = document.LuotTai,
                NgayTaiLen = document.NgayTaiLen,
                MaFile = fileRecord.MaFile,
                TenGoc = fileRecord.TenGoc,
                LoaiFile = fileRecord.LoaiFile,
                DungLuong = fileRecord.DungLuong,
                Extension = fileRecord.Extension
            };
        }

        public async Task<TaiLieuDto> UpdateDocumentAsync(int id, int userId, UpdateDocumentRequest request)
        {
            var document = await _taiLieuRepository.GetQueryable()
                .Include(t => t.NguoiTaiLen)
                .Include(t => t.FileTaiLen)
                .FirstOrDefaultAsync(t => t.MaTaiLieu == id && !t.DaXoa);

            if (document == null)
            {
                throw new NotFoundException("Tài liệu không tồn tại.");
            }

            if (document.MaNguoiTaiLen != userId)
            {
                throw new UnauthorizedException("Bạn chỉ được phép chỉnh sửa tài liệu do chính mình tải lên.");
            }

            document.TieuDe = request.TieuDe;
            document.MoTa = request.MoTa;
            document.NgayCapNhat = DateTime.Now;

            _taiLieuRepository.Update(document);
            await _taiLieuRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã cập nhật tài liệu {DocumentId}", userId, document.MaTaiLieu);

            return new TaiLieuDto
            {
                MaTaiLieu = document.MaTaiLieu,
                MaNhom = document.MaNhom,
                MaNguoiTaiLen = document.MaNguoiTaiLen,
                TenNguoiTaiLen = document.NguoiTaiLen?.HoTen ?? string.Empty,
                TieuDe = document.TieuDe,
                MoTa = document.MoTa ?? string.Empty,
                LuotTai = document.LuotTai,
                NgayTaiLen = document.NgayTaiLen,
                NgayCapNhat = document.NgayCapNhat,
                MaFile = document.MaFile,
                TenGoc = document.FileTaiLen?.TenGoc ?? string.Empty,
                LoaiFile = document.FileTaiLen?.LoaiFile ?? string.Empty,
                DungLuong = document.FileTaiLen?.DungLuong ?? 0,
                Extension = document.FileTaiLen?.Extension ?? string.Empty
            };
        }

        public async Task DeleteDocumentAsync(int id, int userId)
        {
            var document = await _taiLieuRepository.GetQueryable().FirstOrDefaultAsync(t => t.MaTaiLieu == id && !t.DaXoa);
            if (document == null)
            {
                throw new NotFoundException("Tài liệu không tồn tại.");
            }

            if (document.MaNguoiTaiLen != userId)
            {
                throw new UnauthorizedException("Bạn chỉ được phép xóa tài liệu do chính mình tải lên.");
            }

            document.DaXoa = true;
            _taiLieuRepository.Update(document);
            await _taiLieuRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã xóa tài liệu {DocumentId}", userId, document.MaTaiLieu);
        }

        public async Task<(Stream fileStream, string contentType, string fileName)> DownloadDocumentAsync(int id, int userId)
        {
            var document = await _taiLieuRepository.GetQueryable()
                .Include(t => t.FileTaiLen)
                .FirstOrDefaultAsync(t => t.MaTaiLieu == id && !t.DaXoa);

            if (document == null || document.FileTaiLen == null)
            {
                throw new NotFoundException("Tài liệu hoặc tệp tin không tồn tại.");
            }

            await ValidateMemberAsync(userId, document.MaNhom);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents", document.FileTaiLen.TenLuu);
            if (!File.Exists(filePath))
            {
                throw new NotFoundException("Tệp tin vật lý không tồn tại trên hệ thống.");
            }

            document.LuotTai++;
            _taiLieuRepository.Update(document);
            await _taiLieuRepository.SaveAsync();

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (fileStream, document.FileTaiLen.LoaiFile, document.FileTaiLen.TenGoc);
        }

        public async Task<IEnumerable<DocumentGroupDto>> GetMyGroupsAsync(int userId)
        {
            var groups = await _memberRepository.GetQueryable()
                .AsNoTracking()
                .Include(m => m.NhomHocTap)
                .Where(m => m.MaNguoiDung == userId)
                .Select(m => m.NhomHocTap)
                .ToListAsync();

            return groups.Select(g => new DocumentGroupDto
            {
                MaNhom = g.MaNhom,
                TenNhom = g.TenNhom
            });
        }
    }
}
