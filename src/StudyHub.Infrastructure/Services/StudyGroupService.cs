using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.StudyGroup;
using StudyHub.Application.DTOs.Notification;
using StudyHub.Domain.Entities;

using StudyHub.Persistence;

namespace StudyHub.Infrastructure.Services
{
    public class StudyGroupService : IStudyGroupService
    {
        private readonly INhomHocTapRepository _groupRepository;
        private readonly IThanhVienNhomRepository _memberRepository;
        private readonly IGenericRepository<NguoiDung> _userRepository;
        private readonly IGenericRepository<MonHoc> _subjectRepository;
        private readonly ICongViecNhomRepository _groupTaskRepository;
        private readonly INotificationService _notificationService;
        private readonly StudyHubDbContext _dbContext;
        private readonly ILogger<StudyGroupService> _logger;

        public StudyGroupService(
            INhomHocTapRepository groupRepository,
            IThanhVienNhomRepository memberRepository,
            IGenericRepository<NguoiDung> userRepository,
            IGenericRepository<MonHoc> subjectRepository,
            ICongViecNhomRepository groupTaskRepository,
            INotificationService notificationService,
            StudyHubDbContext dbContext,
            ILogger<StudyGroupService> logger)
        {
            _groupRepository = groupRepository;
            _memberRepository = memberRepository;
            _userRepository = userRepository;
            _subjectRepository = subjectRepository;
            _groupTaskRepository = groupTaskRepository;
            _notificationService = notificationService;
            _dbContext = dbContext;
            _logger = logger;
        }

        private static string GenerateGroupCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<IEnumerable<NhomHocTapDto>> GetMyGroupsAsync(int userId, string? search)
        {
            var myGroupIds = await _memberRepository.GetQueryable()
                .AsNoTracking()
                .Where(m => m.MaNguoiDung == userId && m.TrangThai == 1)
                .Select(m => m.MaNhom)
                .ToListAsync();

            var query = _groupRepository.GetQueryable()
                .AsNoTracking()
                .Include(g => g.NguoiTao)
                .Include(g => g.MonHoc)
                .Include(g => g.ThanhVienNhom)
                .Where(g => myGroupIds.Contains(g.MaNhom) && g.TrangThai == 1);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(g => g.TenNhom.ToLower().Contains(s) || (g.MoTa != null && g.MoTa.ToLower().Contains(s)));
            }

            var groups = await query.ToListAsync();

            return groups.Select(g => new NhomHocTapDto
            {
                MaNhom = g.MaNhom,
                MaNguoiTao = g.MaNguoiTao,
                TenNguoiTao = g.NguoiTao?.HoTen ?? string.Empty,
                MaMonHoc = g.MaMonHoc,
                TenMonHoc = g.MonHoc?.TenMonHoc,
                TenNhom = g.TenNhom,
                MoTa = g.MoTa ?? string.Empty,
                AnhDaiDien = g.AnhDaiDien ?? string.Empty,
                MaThamGia = g.MaThamGia,
                SoLuongToiDa = g.SoLuongToiDa,
                SoThanhVienHienTai = g.ThanhVienNhom.Count(m => m.TrangThai == 1),
                TrangThai = g.TrangThai,
                IsOwner = g.MaNguoiTao == userId,
                IsMember = true
            });
        }

        public async Task<NhomHocTapDto> GetGroupByIdAsync(int id, int userId)
        {
            var g = await _groupRepository.GetQueryable()
                .AsNoTracking()
                .Include(x => x.NguoiTao)
                .Include(x => x.MonHoc)
                .Include(x => x.ThanhVienNhom)
                .FirstOrDefaultAsync(x => x.MaNhom == id && x.TrangThai == 1);

            if (g == null)
            {
                throw new NotFoundException("Nhóm học tập không tồn tại hoặc đã bị giải tán.");
            }

            var memberRecord = g.ThanhVienNhom.FirstOrDefault(m => m.MaNguoiDung == userId && m.TrangThai == 1);

            return new NhomHocTapDto
            {
                MaNhom = g.MaNhom,
                MaNguoiTao = g.MaNguoiTao,
                TenNguoiTao = g.NguoiTao?.HoTen ?? string.Empty,
                MaMonHoc = g.MaMonHoc,
                TenMonHoc = g.MonHoc?.TenMonHoc,
                TenNhom = g.TenNhom,
                MoTa = g.MoTa ?? string.Empty,
                AnhDaiDien = g.AnhDaiDien ?? string.Empty,
                MaThamGia = g.MaThamGia,
                SoLuongToiDa = g.SoLuongToiDa,
                SoThanhVienHienTai = g.ThanhVienNhom.Count(m => m.TrangThai == 1),
                TrangThai = g.TrangThai,
                IsOwner = g.MaNguoiTao == userId,
                IsMember = memberRecord != null
            };
        }

        public async Task<NhomHocTapDto> CreateGroupAsync(int userId, CreateStudyGroupRequest request)
        {
            if (request.MaMonHoc.HasValue)
            {
                var subjectExists = await _subjectRepository.GetQueryable().AsNoTracking().AnyAsync(m => m.MaMonHoc == request.MaMonHoc.Value);
                if (!subjectExists)
                {
                    throw new NotFoundException("Môn học được chọn không tồn tại.");
                }
            }

            var groupCode = GenerateGroupCode();
            while (await _groupRepository.GetQueryable().AsNoTracking().AnyAsync(g => g.MaThamGia == groupCode))
            {
                groupCode = GenerateGroupCode();
            }

            var group = new NhomHocTap
            {
                MaNguoiTao = userId,
                MaMonHoc = request.MaMonHoc,
                TenNhom = request.TenNhom,
                MoTa = request.MoTa ?? string.Empty,
                AnhDaiDien = request.AnhDaiDien ?? string.Empty,
                MaThamGia = groupCode,
                SoLuongToiDa = request.SoLuongToiDa,
                TrangThai = 1
            };

            await _groupRepository.AddAsync(group);
            await _groupRepository.SaveAsync();

            var ownerMember = new ThanhVienNhom
            {
                MaNhom = group.MaNhom,
                MaNguoiDung = userId,
                VaiTro = 2, // Owner
                TrangThai = 1,
                NgayThamGia = DateTime.Now
            };

            await _memberRepository.AddAsync(ownerMember);
            await _memberRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã tạo nhóm học tập mới {GroupId}: {TenNhom}", userId, group.MaNhom, group.TenNhom);

            var creator = await _userRepository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(u => u.MaNguoiDung == userId);
            string? subjectName = null;
            if (request.MaMonHoc.HasValue)
            {
                var subject = await _subjectRepository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(m => m.MaMonHoc == request.MaMonHoc.Value);
                subjectName = subject?.TenMonHoc;
            }

            return new NhomHocTapDto
            {
                MaNhom = group.MaNhom,
                MaNguoiTao = userId,
                TenNguoiTao = creator?.HoTen ?? string.Empty,
                MaMonHoc = group.MaMonHoc,
                TenMonHoc = subjectName,
                TenNhom = group.TenNhom,
                MoTa = group.MoTa ?? string.Empty,
                AnhDaiDien = group.AnhDaiDien ?? string.Empty,
                MaThamGia = group.MaThamGia,
                SoLuongToiDa = group.SoLuongToiDa,
                SoThanhVienHienTai = 1,
                TrangThai = 1,
                IsOwner = true,
                IsMember = true
            };
        }

        public async Task<NhomHocTapDto> UpdateGroupAsync(int id, int userId, UpdateStudyGroupRequest request)
        {
            var group = await _groupRepository.GetQueryable()
                .Include(g => g.NguoiTao)
                .Include(g => g.MonHoc)
                .Include(g => g.ThanhVienNhom)
                .FirstOrDefaultAsync(g => g.MaNhom == id && g.TrangThai == 1);

            if (group == null)
            {
                throw new NotFoundException("Nhóm học tập không tồn tại.");
            }

            var member = group.ThanhVienNhom.FirstOrDefault(m => m.MaNguoiDung == userId && m.TrangThai == 1);
            if (member == null || (member.VaiTro != 2 && member.VaiTro != 1))
            {
                throw new UnauthorizedException("Bạn không có quyền quản trị để cập nhật thông tin nhóm này.");
            }

            if (request.MaMonHoc.HasValue)
            {
                var subjectExists = await _subjectRepository.GetQueryable().AsNoTracking().AnyAsync(m => m.MaMonHoc == request.MaMonHoc.Value);
                if (!subjectExists)
                {
                    throw new NotFoundException("Môn học được chọn không tồn tại.");
                }
            }

            group.TenNhom = request.TenNhom;
            group.MoTa = request.MoTa ?? string.Empty;
            group.MaMonHoc = request.MaMonHoc;
            if (!string.IsNullOrEmpty(request.AnhDaiDien))
            {
                group.AnhDaiDien = request.AnhDaiDien;
            }
            group.SoLuongToiDa = request.SoLuongToiDa;

            _groupRepository.Update(group);
            await _groupRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã cập nhật thông tin nhóm học tập {GroupId}", userId, group.MaNhom);

            return new NhomHocTapDto
            {
                MaNhom = group.MaNhom,
                MaNguoiTao = group.MaNguoiTao,
                TenNguoiTao = group.NguoiTao?.HoTen ?? string.Empty,
                MaMonHoc = group.MaMonHoc,
                TenMonHoc = group.MonHoc?.TenMonHoc,
                TenNhom = group.TenNhom,
                MoTa = group.MoTa ?? string.Empty,
                AnhDaiDien = group.AnhDaiDien ?? string.Empty,
                MaThamGia = group.MaThamGia,
                SoLuongToiDa = group.SoLuongToiDa,
                SoThanhVienHienTai = group.ThanhVienNhom.Count(m => m.TrangThai == 1),
                TrangThai = group.TrangThai,
                IsOwner = group.MaNguoiTao == userId,
                IsMember = true
            };
        }

        public async Task DeleteGroupAsync(int id, int userId)
        {
            var group = await _groupRepository.GetQueryable().FirstOrDefaultAsync(g => g.MaNhom == id && g.TrangThai == 1);
            if (group == null)
            {
                throw new NotFoundException("Nhóm học tập không tồn tại.");
            }

            if (group.MaNguoiTao != userId)
            {
                throw new UnauthorizedException("Chỉ người tạo nhóm mới có quyền giải tán nhóm học tập.");
            }

            group.TrangThai = 0; // Dissolved
            _groupRepository.Update(group);
            await _groupRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã giải tán nhóm học tập {GroupId}", userId, id);
        }

        public async Task<NhomHocTapDto> JoinGroupViaCodeAsync(int userId, JoinGroupRequest request)
        {
            var code = request.MaThamGia.Trim().ToUpper();
            var group = await _groupRepository.GetQueryable()
                .Include(g => g.NguoiTao)
                .Include(g => g.MonHoc)
                .Include(g => g.ThanhVienNhom)
                .FirstOrDefaultAsync(g => g.MaThamGia == code && g.TrangThai == 1);

            if (group == null)
            {
                throw new NotFoundException("Mã tham gia nhóm không chính xác hoặc nhóm đã bị giải tán.");
            }

            var existingMember = group.ThanhVienNhom.FirstOrDefault(m => m.MaNguoiDung == userId);
            if (existingMember != null)
            {
                if (existingMember.TrangThai == 1)
                {
                    return new NhomHocTapDto
                    {
                        MaNhom = group.MaNhom,
                        MaNguoiTao = group.MaNguoiTao,
                        TenNguoiTao = group.NguoiTao?.HoTen ?? string.Empty,
                        MaMonHoc = group.MaMonHoc,
                        TenMonHoc = group.MonHoc?.TenMonHoc,
                        TenNhom = group.TenNhom,
                        MoTa = group.MoTa ?? string.Empty,
                        AnhDaiDien = group.AnhDaiDien ?? string.Empty,
                        MaThamGia = group.MaThamGia,
                        SoLuongToiDa = group.SoLuongToiDa,
                        SoThanhVienHienTai = group.ThanhVienNhom.Count(m => m.TrangThai == 1),
                        TrangThai = group.TrangThai,
                        IsOwner = group.MaNguoiTao == userId,
                        IsMember = true
                    };
                }

                existingMember.TrangThai = 1;
                existingMember.NgayThamGia = DateTime.Now;
                _memberRepository.Update(existingMember);
            }
            else
            {
                var activeMemberCount = group.ThanhVienNhom.Count(m => m.TrangThai == 1);
                if (activeMemberCount >= group.SoLuongToiDa)
                {
                    throw new BadRequestException("Nhóm học tập đã đạt số lượng thành viên tối đa.");
                }

                var newMember = new ThanhVienNhom
                {
                    MaNhom = group.MaNhom,
                    MaNguoiDung = userId,
                    VaiTro = 0, // Member
                    TrangThai = 1,
                    NgayThamGia = DateTime.Now
                };

                await _memberRepository.AddAsync(newMember);
            }

            await _memberRepository.SaveAsync();

            // Trigger Realtime Notification for Group Creator/Admin
            if (group.MaNguoiTao != userId)
            {
                try
                {
                    var joiningUser = await _userRepository.GetByIdAsync(userId);
                    await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                    {
                        MaNguoiDung = group.MaNguoiTao,
                        MaLoaiThongBao = 3, // Nhóm học tập
                        TieuDe = $"Thành viên mới trong nhóm {group.TenNhom}",
                        NoiDung = $"Người dùng {joiningUser?.HoTen ?? "Một thành viên"} đã gia nhập nhóm \"{group.TenNhom}\"",
                        DuongDan = $"/groups/{group.MaNhom}",
                        MucDo = 1
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể phát thông báo tham gia nhóm cho trưởng nhóm {UserId}", group.MaNguoiTao);
                }
            }

            _logger.LogInformation("Người dùng {UserId} đã tham gia nhóm học tập {GroupId} bằng mã {Code}", userId, group.MaNhom, code);

            return new NhomHocTapDto
            {
                MaNhom = group.MaNhom,
                MaNguoiTao = group.MaNguoiTao,
                TenNguoiTao = group.NguoiTao?.HoTen ?? string.Empty,
                MaMonHoc = group.MaMonHoc,
                TenMonHoc = group.MonHoc?.TenMonHoc,
                TenNhom = group.TenNhom,
                MoTa = group.MoTa ?? string.Empty,
                AnhDaiDien = group.AnhDaiDien ?? string.Empty,
                MaThamGia = group.MaThamGia,
                SoLuongToiDa = group.SoLuongToiDa,
                SoThanhVienHienTai = group.ThanhVienNhom.Count(m => m.TrangThai == 1) + (existingMember == null ? 1 : 0),
                TrangThai = group.TrangThai,
                IsOwner = group.MaNguoiTao == userId,
                IsMember = true
            };
        }

        public async Task LeaveGroupAsync(int id, int userId)
        {
            var member = await _memberRepository.GetQueryable()
                .FirstOrDefaultAsync(m => m.MaNhom == id && m.MaNguoiDung == userId && m.TrangThai == 1);

            if (member == null)
            {
                throw new NotFoundException("Bạn không phải là thành viên của nhóm học tập này.");
            }

            if (member.VaiTro == 2)
            {
                var otherMemberCount = await _memberRepository.GetQueryable()
                    .CountAsync(m => m.MaNhom == id && m.MaNguoiDung != userId && m.TrangThai == 1);

                if (otherMemberCount > 0)
                {
                    throw new BadRequestException("Trưởng nhóm không thể rời nhóm khi vẫn còn các thành viên khác. Vui lòng chuyển quyền trưởng nhóm hoặc giải tán nhóm.");
                }
            }

            member.TrangThai = 0; // Left
            _memberRepository.Update(member);
            await _memberRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã rời nhóm học tập {GroupId}", userId, id);
        }

        public async Task<IEnumerable<ThanhVienNhomDto>> GetGroupMembersAsync(int id, int userId)
        {
            var isMember = await _memberRepository.GetQueryable().AsNoTracking()
                .AnyAsync(m => m.MaNhom == id && m.MaNguoiDung == userId && m.TrangThai == 1);

            if (!isMember)
            {
                throw new UnauthorizedException("Bạn không có quyền xem danh sách thành viên của nhóm này.");
            }

            var members = await _memberRepository.GetQueryable()
                .AsNoTracking()
                .Include(m => m.NguoiDung)
                .Where(m => m.MaNhom == id && m.TrangThai == 1)
                .OrderByDescending(m => m.VaiTro)
                .ThenBy(m => m.NgayThamGia)
                .ToListAsync();

            return members.Select(m => new ThanhVienNhomDto
            {
                MaThanhVien = m.MaThanhVien,
                MaNhom = m.MaNhom,
                MaNguoiDung = m.MaNguoiDung,
                HoTen = m.NguoiDung?.HoTen ?? string.Empty,
                Email = m.NguoiDung?.Email ?? string.Empty,
                Avatar = m.NguoiDung?.AnhDaiDien,
                VaiTro = m.VaiTro,
                TrangThai = m.TrangThai,
                NgayThamGia = m.NgayThamGia
            });
        }

        public async Task<ThanhVienNhomDto> AddMemberAsync(int id, int memberUserId, int currentUserId)
        {
            var currentMember = await _memberRepository.GetQueryable().AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaNhom == id && m.MaNguoiDung == currentUserId && m.TrangThai == 1);

            if (currentMember == null || (currentMember.VaiTro != 2 && currentMember.VaiTro != 1))
            {
                throw new UnauthorizedException("Bạn không có quyền thêm thành viên vào nhóm này.");
            }

            var group = await _groupRepository.GetQueryable().AsNoTracking()
                .Include(g => g.ThanhVienNhom)
                .FirstOrDefaultAsync(g => g.MaNhom == id && g.TrangThai == 1);

            if (group == null)
            {
                throw new NotFoundException("Nhóm học tập không tồn tại.");
            }

            var userExists = await _userRepository.GetQueryable().AsNoTracking().AnyAsync(u => u.MaNguoiDung == memberUserId);
            if (!userExists)
            {
                throw new NotFoundException("Người dùng cần thêm không tồn tại.");
            }

            var existingMember = group.ThanhVienNhom.FirstOrDefault(m => m.MaNguoiDung == memberUserId);
            if (existingMember != null)
            {
                if (existingMember.TrangThai == 1)
                {
                    throw new BadRequestException("Người dùng đã là thành viên của nhóm.");
                }

                existingMember.TrangThai = 1;
                existingMember.NgayThamGia = DateTime.Now;
                _memberRepository.Update(existingMember);
            }
            else
            {
                var activeCount = group.ThanhVienNhom.Count(m => m.TrangThai == 1);
                if (activeCount >= group.SoLuongToiDa)
                {
                    throw new BadRequestException("Nhóm học tập đã đạt số lượng thành viên tối đa.");
                }

                var newMember = new ThanhVienNhom
                {
                    MaNhom = id,
                    MaNguoiDung = memberUserId,
                    VaiTro = 0,
                    TrangThai = 1,
                    NgayThamGia = DateTime.Now
                };

                await _memberRepository.AddAsync(newMember);
            }

            await _memberRepository.SaveAsync();

            // Trigger Realtime Notification for Added Member
            if (memberUserId != currentUserId)
            {
                try
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                    {
                        MaNguoiDung = memberUserId,
                        MaLoaiThongBao = 3, // Nhóm học tập
                        TieuDe = $"Bạn đã được thêm vào nhóm {group?.TenNhom ?? string.Empty}",
                        NoiDung = $"Bạn đã trở thành thành viên của nhóm học tập \"{group?.TenNhom}\"",
                        DuongDan = $"/groups/{id}",
                        MucDo = 1
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể gửi thông báo thêm thành viên cho người dùng {UserId}", memberUserId);
                }
            }

            var targetUser = await _userRepository.GetQueryable().AsNoTracking().FirstAsync(u => u.MaNguoiDung == memberUserId);

            return new ThanhVienNhomDto
            {
                MaNhom = id,
                MaNguoiDung = memberUserId,
                HoTen = targetUser.HoTen,
                Email = targetUser.Email,
                Avatar = targetUser.AnhDaiDien,
                VaiTro = 0,
                TrangThai = 1,
                NgayThamGia = DateTime.Now
            };
        }

        public async Task RemoveMemberAsync(int id, int memberUserId, int currentUserId)
        {
            var currentMember = await _memberRepository.GetQueryable().AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaNhom == id && m.MaNguoiDung == currentUserId && m.TrangThai == 1);

            if (currentMember == null || (currentMember.VaiTro != 2 && currentMember.VaiTro != 1))
            {
                throw new UnauthorizedException("Bạn không có quyền loại bỏ thành viên khỏi nhóm này.");
            }

            var targetMember = await _memberRepository.GetQueryable()
                .FirstOrDefaultAsync(m => m.MaNhom == id && m.MaNguoiDung == memberUserId && m.TrangThai == 1);

            if (targetMember == null)
            {
                throw new NotFoundException("Thành viên không tồn tại trong nhóm này.");
            }

            if (targetMember.VaiTro == 2)
            {
                throw new BadRequestException("Không thể loại bỏ trưởng nhóm.");
            }

            targetMember.TrangThai = 0;
            _memberRepository.Update(targetMember);
            await _memberRepository.SaveAsync();

            _logger.LogInformation("Người dùng {UserId} đã xóa thành viên {TargetUserId} khỏi nhóm {GroupId}", currentUserId, memberUserId, id);
        }

        public async Task<List<GroupTaskDto>> GetGroupTasksAsync(int groupId, int userId)
        {
            var isMember = await _memberRepository.GetQueryable()
                .AsNoTracking()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);

            if (!isMember)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập công việc của nhóm này.");
            }

            var tasks = await _groupTaskRepository.GetQueryable()
                .AsNoTracking()
                .Include(t => t.NguoiTao)
                .Include(t => t.NguoiDuocGiao)
                .Where(t => t.MaNhomHocTap == groupId && !t.DaXoa)
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            return tasks.Select(t => new GroupTaskDto
            {
                MaCongViec = t.MaCongViecNhom,
                MaNhomHocTap = t.MaNhomHocTap,
                TieuDe = t.TieuDe,
                MoTa = t.MoTa ?? string.Empty,
                DoUuTien = t.DoUuTien,
                TrangThai = t.TrangThai,
                NgayBatDau = null,
                HanHoanThanh = t.HanHoanThanh,
                MaNguoiDuocGiao = t.MaNguoiDuocGiao,
                TenNguoiDuocGiao = t.NguoiDuocGiao?.HoTen,
                AnhNguoiDuocGiao = t.NguoiDuocGiao?.AnhDaiDien,
                NguoiTaoId = t.MaNguoiTao,
                TenNguoiTao = t.NguoiTao?.HoTen,
                AnhNguoiTao = t.NguoiTao?.AnhDaiDien,
                NgayTao = t.NgayTao
            }).ToList();
        }

        public async Task<GroupTaskDto> CreateGroupTaskAsync(int groupId, int userId, CreateGroupTaskRequest request)
        {
            var isMember = await _memberRepository.GetQueryable()
                .AsNoTracking()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);

            if (!isMember)
            {
                throw new ForbiddenException("Bạn không có quyền tạo công việc trong nhóm này.");
            }

            var task = new CongViecNhom
            {
                MaNhomHocTap = groupId,
                MaNguoiTao = userId,
                TieuDe = request.TieuDe,
                MoTa = request.MoTa ?? string.Empty,
                DoUuTien = request.DoUuTien,
                TrangThai = request.TrangThai,
                HanHoanThanh = request.HanHoanThanh,
                MaNguoiDuocGiao = request.MaNguoiDuocGiao,
                NgayTao = DateTime.Now,
                DaXoa = false
            };

            await _groupTaskRepository.AddAsync(task);
            await _groupTaskRepository.SaveAsync();

            // Trigger Realtime Notification for Assigned Member or Creator
            if (request.MaNguoiDuocGiao.HasValue && request.MaNguoiDuocGiao.Value != userId)
            {
                try
                {
                    var group = await _groupRepository.GetByIdAsync(groupId);
                    await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                    {
                        MaNguoiDung = request.MaNguoiDuocGiao.Value,
                        MaLoaiThongBao = 1, // Công việc
                        TieuDe = $"Bạn có công việc mới trong nhóm {group?.TenNhom ?? string.Empty}",
                        NoiDung = $"Bạn được giao công việc: \"{request.TieuDe}\"",
                        DuongDan = $"/groups/{groupId}",
                        MucDo = (byte)(request.DoUuTien == 2 ? 2 : 1)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể gửi thông báo phân công công việc cho người dùng {UserId}", request.MaNguoiDuocGiao.Value);
                }
            }
            else
            {
                try
                {
                    var group = await _groupRepository.GetByIdAsync(groupId);
                    await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                    {
                        MaNguoiDung = userId,
                        MaLoaiThongBao = 1, // Công việc
                        TieuDe = $"Đã tạo công việc mới trong nhóm {group?.TenNhom ?? string.Empty}",
                        NoiDung = $"Bạn vừa tạo công việc nhóm: \"{request.TieuDe}\"",
                        DuongDan = $"/groups/{groupId}",
                        MucDo = (byte)(request.DoUuTien == 2 ? 2 : 1)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể gửi thông báo tạo công việc nhóm cho người dùng {UserId}", userId);
                }
            }

            var createdList = await GetGroupTasksAsync(groupId, userId);
            return createdList.First(t => t.MaCongViec == task.MaCongViecNhom);
        }

        public async Task<GroupTaskDto> UpdateGroupTaskStatusAsync(int groupId, int taskId, int userId, byte status)
        {
            var isMember = await _memberRepository.GetQueryable()
                .AsNoTracking()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);

            if (!isMember)
            {
                throw new ForbiddenException("Bạn không có quyền cập nhật công việc trong nhóm này.");
            }

            var task = await _groupTaskRepository.GetByIdAsync(taskId);
            if (task == null || task.MaNhomHocTap != groupId || task.DaXoa)
            {
                throw new NotFoundException("Công việc không tồn tại trong nhóm này.");
            }

            task.TrangThai = status;
            task.NgayCapNhat = DateTime.Now;

            _groupTaskRepository.Update(task);
            await _groupTaskRepository.SaveAsync();

            var updatedList = await GetGroupTasksAsync(groupId, userId);
            return updatedList.First(t => t.MaCongViec == task.MaCongViecNhom);
        }

        public async Task DeleteGroupTaskAsync(int groupId, int taskId, int userId)
        {
            var isMember = await _memberRepository.GetQueryable()
                .AsNoTracking()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);

            if (!isMember)
            {
                throw new ForbiddenException("Bạn không có quyền xóa công việc trong nhóm này.");
            }

            var task = await _groupTaskRepository.GetByIdAsync(taskId);
            if (task == null || task.MaNhomHocTap != groupId || task.DaXoa)
            {
                throw new NotFoundException("Công việc không tồn tại.");
            }

            task.DaXoa = true;
            task.NgayCapNhat = DateTime.Now;
            _groupTaskRepository.Update(task);
            await _groupTaskRepository.SaveAsync();
        }
        public async Task<List<LichHopNhomDto>> GetGroupMeetingsAsync(int groupId, int userId)
        {
            var isMember = await _memberRepository.GetQueryable()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);
            if (!isMember) throw new UnauthorizedException("Bạn không phải là thành viên của nhóm này.");

            var meetings = await _dbContext.LichHopNhom
                .Include(m => m.NguoiTao)
                .Where(m => m.MaNhom == groupId && m.TrangThai == 1)
                .OrderBy(m => m.ThoiGianBatDau)
                .Select(m => new LichHopNhomDto
                {
                    MaLichHop = m.MaLichHop,
                    MaNhom = m.MaNhom,
                    MaNguoiTao = m.MaNguoiTao,
                    TenNguoiTao = m.NguoiTao.HoTen,
                    TieuDe = m.TieuDe,
                    MoTa = m.MoTa,
                    NenTang = m.NenTang,
                    DuongDan = m.DuongDan,
                    ThoiGianBatDau = m.ThoiGianBatDau,
                    ThoiGianKetThuc = m.ThoiGianKetThuc,
                    TrangThai = m.TrangThai
                })
                .ToListAsync();

            return meetings;
        }

        public async Task<LichHopNhomDto> CreateGroupMeetingAsync(int groupId, int userId, CreateLichHopRequest request)
        {
            var isMember = await _memberRepository.GetQueryable()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền tạo lịch họp cho nhóm này.");

            var meeting = new LichHopNhom
            {
                MaNhom = groupId,
                MaNguoiTao = userId,
                TieuDe = request.TieuDe,
                MoTa = request.MoTa,
                NenTang = request.NenTang,
                DuongDan = request.DuongDan,
                ThoiGianBatDau = request.ThoiGianBatDau,
                ThoiGianKetThuc = request.ThoiGianKetThuc,
                TrangThai = 1
            };

            _dbContext.LichHopNhom.Add(meeting);
            await _dbContext.SaveChangesAsync();

            // Trigger Realtime Notifications for Group Members
            try
            {
                var otherMembers = await _memberRepository.GetQueryable().AsNoTracking()
                    .Where(m => m.MaNhom == groupId && m.MaNguoiDung != userId && m.TrangThai == 1)
                    .Select(m => m.MaNguoiDung)
                    .ToListAsync();

                var group = await _groupRepository.GetByIdAsync(groupId);
                foreach (var memberId in otherMembers)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                    {
                        MaNguoiDung = memberId,
                        MaLoaiThongBao = 2, // Lịch học / Lịch họp
                        TieuDe = $"Lịch họp mới trong nhóm {group?.TenNhom ?? string.Empty}",
                        NoiDung = $"Lịch họp: \"{request.TieuDe}\" vào {request.ThoiGianBatDau:HH:mm dd/MM/yyyy}",
                        DuongDan = $"/groups/{groupId}",
                        MucDo = 1
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể phát thông báo lịch họp mới cho các thành viên trong nhóm {GroupId}", groupId);
            }

            var creator = await _userRepository.GetByIdAsync(userId);

            return new LichHopNhomDto
            {
                MaLichHop = meeting.MaLichHop,
                MaNhom = meeting.MaNhom,
                MaNguoiTao = meeting.MaNguoiTao,
                TenNguoiTao = creator?.HoTen ?? "Người dùng",
                TieuDe = meeting.TieuDe,
                MoTa = meeting.MoTa,
                NenTang = meeting.NenTang,
                DuongDan = meeting.DuongDan,
                ThoiGianBatDau = meeting.ThoiGianBatDau,
                ThoiGianKetThuc = meeting.ThoiGianKetThuc,
                TrangThai = meeting.TrangThai
            };
        }

        public async Task<LichHopNhomDto> UpdateGroupMeetingAsync(int groupId, int meetingId, int userId, CreateLichHopRequest request)
        {
            var isMember = await _memberRepository.GetQueryable()
                .AsNoTracking()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền chỉnh sửa lịch họp của nhóm này.");

            var meeting = await _dbContext.LichHopNhom
                .Include(m => m.NguoiTao)
                .FirstOrDefaultAsync(m => m.MaLichHop == meetingId && m.MaNhom == groupId && !m.DaXoa);

            if (meeting == null) throw new NotFoundException("Lịch họp không tồn tại.");

            meeting.TieuDe = request.TieuDe;
            meeting.MoTa = request.MoTa ?? string.Empty;
            meeting.NenTang = request.NenTang;
            meeting.DuongDan = request.DuongDan;
            meeting.ThoiGianBatDau = request.ThoiGianBatDau;
            meeting.ThoiGianKetThuc = request.ThoiGianKetThuc;
            meeting.NgayCapNhat = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return new LichHopNhomDto
            {
                MaLichHop = meeting.MaLichHop,
                MaNhom = meeting.MaNhom,
                MaNguoiTao = meeting.MaNguoiTao,
                TenNguoiTao = meeting.NguoiTao?.HoTen ?? "Người dùng",
                TieuDe = meeting.TieuDe,
                MoTa = meeting.MoTa,
                NenTang = meeting.NenTang,
                DuongDan = meeting.DuongDan,
                ThoiGianBatDau = meeting.ThoiGianBatDau,
                ThoiGianKetThuc = meeting.ThoiGianKetThuc,
                TrangThai = meeting.TrangThai
            };
        }

        public async Task DeleteGroupMeetingAsync(int groupId, int meetingId, int userId)
        {
            var isMember = await _memberRepository.GetQueryable()
                .AnyAsync(m => m.MaNhom == groupId && m.MaNguoiDung == userId && m.TrangThai == 1);
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền xóa lịch họp của nhóm này.");

            var meeting = await _dbContext.LichHopNhom
                .FirstOrDefaultAsync(m => m.MaLichHop == meetingId && m.MaNhom == groupId && m.TrangThai == 1);

            if (meeting == null) throw new NotFoundException("Lịch họp không tồn tại.");

            meeting.TrangThai = 0;
            meeting.DaXoa = true;
            await _dbContext.SaveChangesAsync();
        }

        // ── Group Folders & Documents ──
        public async Task<List<ThuMucTaiLieuDto>> GetGroupFoldersAsync(int groupId, int userId)
        {
            var isMember = await _dbContext.NhomHocTap
                .AsNoTracking()
                .AnyAsync(g => g.MaNhom == groupId && !g.DaXoa && (g.MaNguoiTao == userId || g.ThanhVienNhom.Any(m => m.MaNguoiDung == userId && m.TrangThai == 1)));
            if (!isMember) throw new UnauthorizedException("Bạn không phải thành viên nhóm.");

            var folders = await _dbContext.ThuMucTaiLieu
                .AsNoTracking()
                .Where(f => f.MaNhom == groupId && !f.DaXoa)
                .OrderBy(f => f.TenThuMuc)
                .Select(f => new ThuMucTaiLieuDto
                {
                    MaThuMuc = f.MaThuMuc,
                    MaNhom = f.MaNhom,
                    MaNguoiTao = f.MaNguoiTao,
                    TenThuMuc = f.TenThuMuc,
                    MoTa = f.MoTa,
                    NgayTao = f.NgayTao,
                    SoLuongFile = _dbContext.TaiLieu.Count(d => d.MaThuMuc == f.MaThuMuc && !d.DaXoa)
                })
                .ToListAsync();

            return folders;
        }

        public async Task<ThuMucTaiLieuDto> CreateGroupFolderAsync(int groupId, int userId, CreateThuMucRequest request)
        {
            var isMember = await _dbContext.NhomHocTap
                .AsNoTracking()
                .AnyAsync(g => g.MaNhom == groupId && !g.DaXoa && (g.MaNguoiTao == userId || g.ThanhVienNhom.Any(m => m.MaNguoiDung == userId && m.TrangThai == 1)));
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền tạo thư mục trong nhóm này.");

            var folder = new ThuMucTaiLieu
            {
                MaNhom = groupId,
                MaNguoiTao = userId,
                TenThuMuc = request.TenThuMuc,
                MoTa = request.MoTa,
                NgayTao = DateTime.Now,
                DaXoa = false
            };

            _dbContext.ThuMucTaiLieu.Add(folder);
            await _dbContext.SaveChangesAsync();

            return new ThuMucTaiLieuDto
            {
                MaThuMuc = folder.MaThuMuc,
                MaNhom = folder.MaNhom,
                MaNguoiTao = folder.MaNguoiTao,
                TenThuMuc = folder.TenThuMuc,
                MoTa = folder.MoTa,
                NgayTao = folder.NgayTao,
                SoLuongFile = 0
            };
        }

        public async Task<ThuMucTaiLieuDto> UpdateGroupFolderAsync(int groupId, int folderId, int userId, UpdateThuMucRequest request)
        {
            var isMember = await _dbContext.NhomHocTap
                .AsNoTracking()
                .AnyAsync(g => g.MaNhom == groupId && !g.DaXoa && (g.MaNguoiTao == userId || g.ThanhVienNhom.Any(m => m.MaNguoiDung == userId && m.TrangThai == 1)));
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền chỉnh sửa thư mục này.");

            var folder = await _dbContext.ThuMucTaiLieu
                .FirstOrDefaultAsync(f => f.MaThuMuc == folderId && f.MaNhom == groupId && !f.DaXoa);
            if (folder == null) throw new NotFoundException("Thư mục không tồn tại.");

            folder.TenThuMuc = request.TenThuMuc;
            if (request.MoTa != null) folder.MoTa = request.MoTa;
            folder.NgayCapNhat = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            var count = await _dbContext.TaiLieu.CountAsync(d => d.MaThuMuc == folderId && !d.DaXoa);

            return new ThuMucTaiLieuDto
            {
                MaThuMuc = folder.MaThuMuc,
                MaNhom = folder.MaNhom,
                MaNguoiTao = folder.MaNguoiTao,
                TenThuMuc = folder.TenThuMuc,
                MoTa = folder.MoTa,
                NgayTao = folder.NgayTao,
                SoLuongFile = count
            };
        }

        public async Task DeleteGroupFolderAsync(int groupId, int folderId, int userId)
        {
            var isMember = await _dbContext.NhomHocTap
                .AsNoTracking()
                .AnyAsync(g => g.MaNhom == groupId && !g.DaXoa && (g.MaNguoiTao == userId || g.ThanhVienNhom.Any(m => m.MaNguoiDung == userId && m.TrangThai == 1)));
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền xóa thư mục này.");

            var folder = await _dbContext.ThuMucTaiLieu
                .FirstOrDefaultAsync(f => f.MaThuMuc == folderId && f.MaNhom == groupId && !f.DaXoa);
            if (folder == null) throw new NotFoundException("Thư mục không tồn tại.");

            folder.DaXoa = true;
            folder.NgayCapNhat = DateTime.Now;

            // Chuyển các tài liệu trong thư mục bị xóa về trạng thái không thuộc thư mục nào (MaThuMuc = null) để giữ nguyên file
            var docsInFolder = await _dbContext.TaiLieu
                .Where(d => d.MaThuMuc == folderId && !d.DaXoa)
                .ToListAsync();

            foreach (var doc in docsInFolder)
            {
                doc.MaThuMuc = null;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<GroupDocumentDto>> GetGroupDocumentsAsync(int groupId, int? folderId, int userId)
        {
            var isMember = await _dbContext.NhomHocTap
                .AsNoTracking()
                .AnyAsync(g => g.MaNhom == groupId && !g.DaXoa && (g.MaNguoiTao == userId || g.ThanhVienNhom.Any(m => m.MaNguoiDung == userId && m.TrangThai == 1)));
            if (!isMember) throw new UnauthorizedException("Bạn không phải thành viên nhóm.");

            var query = _dbContext.TaiLieu
                .AsNoTracking()
                .Include(d => d.NguoiTaiLen)
                .Include(d => d.FileTaiLen)
                .Include(d => d.ThuMucTaiLieu)
                .Where(d => d.MaNhom == groupId && !d.DaXoa);

            if (folderId.HasValue && folderId.Value > 0)
            {
                query = query.Where(d => d.MaThuMuc == folderId.Value);
            }

            var documents = await query
                .OrderByDescending(d => d.NgayTaiLen)
                .Select(d => new GroupDocumentDto
                {
                    MaTaiLieu = d.MaTaiLieu,
                    MaNhom = d.MaNhom,
                    MaThuMuc = d.MaThuMuc,
                    TenThuMuc = d.ThuMucTaiLieu != null ? d.ThuMucTaiLieu.TenThuMuc : null,
                    MaNguoiTaiLen = d.MaNguoiTaiLen,
                    TenNguoiTaiLen = d.NguoiTaiLen != null ? d.NguoiTaiLen.HoTen : "Thành viên",
                    AvatarNguoiTaiLen = d.NguoiTaiLen != null ? d.NguoiTaiLen.AnhDaiDien : null,
                    TieuDe = d.TieuDe,
                    MoTa = d.MoTa,
                    DuongDanFile = d.FileTaiLen != null ? d.FileTaiLen.DuongDan : string.Empty,
                    Extension = d.FileTaiLen != null ? d.FileTaiLen.Extension : string.Empty,
                    DungLuong = d.FileTaiLen != null ? d.FileTaiLen.DungLuong : 0,
                    NgayTaiLen = d.NgayTaiLen
                })
                .ToListAsync();

            return documents;
        }

        public async Task<GroupDocumentDto> CreateGroupDocumentAsync(int groupId, int userId, CreateGroupDocumentRequest request)
        {
            var isMember = await _dbContext.NhomHocTap
                .AsNoTracking()
                .AnyAsync(g => g.MaNhom == groupId && !g.DaXoa && (g.MaNguoiTao == userId || g.ThanhVienNhom.Any(m => m.MaNguoiDung == userId && m.TrangThai == 1)));
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền tải lên tài liệu trong nhóm này.");

            var fileEntity = new FileTaiLen
            {
                MaNguoiDung = userId,
                TenGoc = request.TieuDe,
                TenLuu = Guid.NewGuid().ToString() + (request.Extension ?? ".dat"),
                DuongDan = request.DuongDanFile,
                LoaiFile = request.Extension ?? string.Empty,
                Extension = request.Extension ?? string.Empty,
                DungLuong = request.DungLuong,
                NgayTaiLen = DateTime.Now,
                DaXoa = false
            };

            _dbContext.FileTaiLen.Add(fileEntity);
            await _dbContext.SaveChangesAsync();

            var docEntity = new TaiLieu
            {
                MaNhom = groupId,
                MaNguoiTaiLen = userId,
                MaFile = fileEntity.MaFile,
                MaThuMuc = request.MaThuMuc,
                TieuDe = request.TieuDe,
                MoTa = request.MoTa ?? string.Empty,
                LuotTai = 0,
                NgayTaiLen = DateTime.Now,
                DaXoa = false
            };

            _dbContext.TaiLieu.Add(docEntity);
            await _dbContext.SaveChangesAsync();

            var user = await _userRepository.GetByIdAsync(userId);
            string? folderName = null;
            if (request.MaThuMuc.HasValue)
            {
                var folder = await _dbContext.ThuMucTaiLieu.FindAsync(request.MaThuMuc.Value);
                folderName = folder?.TenThuMuc;
            }

            // Trigger Realtime Notification for Group Members
            try
            {
                var otherMembers = await _memberRepository.GetQueryable().AsNoTracking()
                    .Where(m => m.MaNhom == groupId && m.MaNguoiDung != userId && m.TrangThai == 1)
                    .Select(m => m.MaNguoiDung)
                    .ToListAsync();

                var group = await _groupRepository.GetByIdAsync(groupId);
                foreach (var memberId in otherMembers)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                    {
                        MaNguoiDung = memberId,
                        MaLoaiThongBao = 3, // Nhóm
                        TieuDe = $"Tài liệu mới trong nhóm {group?.TenNhom ?? string.Empty}",
                        NoiDung = $"📁 {user?.HoTen ?? "Một thành viên"} vừa tải lên tài liệu mới \"{request.TieuDe}\" trong nhóm",
                        DuongDan = $"/groups/{groupId}",
                        MucDo = 1
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể phát thông báo tài liệu mới cho các thành viên nhóm {GroupId}", groupId);
            }

            return new GroupDocumentDto
            {
                MaTaiLieu = docEntity.MaTaiLieu,
                MaNhom = docEntity.MaNhom,
                MaThuMuc = docEntity.MaThuMuc,
                TenThuMuc = folderName,
                MaNguoiTaiLen = docEntity.MaNguoiTaiLen,
                TenNguoiTaiLen = user?.HoTen ?? "Người dùng",
                AvatarNguoiTaiLen = user?.AnhDaiDien,
                TieuDe = docEntity.TieuDe,
                MoTa = docEntity.MoTa,
                DuongDanFile = fileEntity.DuongDan,
                Extension = fileEntity.Extension,
                DungLuong = fileEntity.DungLuong,
                NgayTaiLen = docEntity.NgayTaiLen
            };
        }

        public async Task DeleteGroupDocumentAsync(int groupId, int documentId, int userId)
        {
            var isMember = await _dbContext.NhomHocTap
                .AsNoTracking()
                .AnyAsync(g => g.MaNhom == groupId && !g.DaXoa && (g.MaNguoiTao == userId || g.ThanhVienNhom.Any(m => m.MaNguoiDung == userId && m.TrangThai == 1)));
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền xóa tài liệu này.");

            var doc = await _dbContext.TaiLieu.FirstOrDefaultAsync(d => d.MaTaiLieu == documentId && d.MaNhom == groupId && !d.DaXoa);
            if (doc == null) throw new NotFoundException("Tài liệu không tồn tại.");

            doc.DaXoa = true;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<(byte[] fileBytes, string contentType, string fileName)> DownloadGroupDocumentAsync(int groupId, int documentId, int userId)
        {
            var isMember = await _dbContext.NhomHocTap
                .AsNoTracking()
                .AnyAsync(g => g.MaNhom == groupId && !g.DaXoa && (g.MaNguoiTao == userId || g.ThanhVienNhom.Any(m => m.MaNguoiDung == userId && m.TrangThai == 1)));
            if (!isMember) throw new UnauthorizedException("Bạn không có quyền tải xuống tài liệu này.");

            var doc = await _dbContext.TaiLieu
                .AsNoTracking()
                .Include(d => d.FileTaiLen)
                .FirstOrDefaultAsync(d => d.MaTaiLieu == documentId && d.MaNhom == groupId && !d.DaXoa);

            if (doc == null) throw new NotFoundException("Tài liệu không tồn tại.");

            var fileEntity = doc.FileTaiLen;
            string fileName = doc.TieuDe ?? "tai_lieu";
            string extension = fileEntity?.Extension ?? ".pdf";
            if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                fileName += extension;
            }

            string contentType = GetContentTypeByExtension(extension);
            byte[] fileBytes = null;

            if (fileEntity != null)
            {
                string pathCandidate = fileEntity.DuongDan ?? string.Empty;
                if (System.IO.File.Exists(pathCandidate))
                {
                    fileBytes = await System.IO.File.ReadAllBytesAsync(pathCandidate);
                }
                else
                {
                    string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pathCandidate.TrimStart('/', '\\'));
                    if (System.IO.File.Exists(wwwrootPath))
                    {
                        fileBytes = await System.IO.File.ReadAllBytesAsync(wwwrootPath);
                    }
                    else
                    {
                        string uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", fileEntity.TenLuu ?? string.Empty);
                        if (System.IO.File.Exists(uploadsPath))
                        {
                            fileBytes = await System.IO.File.ReadAllBytesAsync(uploadsPath);
                        }
                    }
                }
            }

            if (fileBytes == null || fileBytes.Length == 0)
            {
                // Fallback for sample/demo files without physical storage on disk
                string dummyContent = $"[StudyHub Document]\r\nTên tài liệu: {doc.TieuDe}\r\nMô tả: {doc.MoTa}\r\nNgày tải lên: {doc.NgayTaiLen:dd/MM/yyyy HH:mm}";
                fileBytes = System.Text.Encoding.UTF8.GetBytes(dummyContent);
            }

            // Tăng lượt tải
            doc.LuotTai += 1;
            await _dbContext.SaveChangesAsync();

            return (fileBytes, contentType, fileName);
        }

        private string GetContentTypeByExtension(string ext)
        {
            ext = (ext ?? "").ToLower().TrimStart('.');
            return ext switch
            {
                "pdf" => "application/pdf",
                "doc" or "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xls" or "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ppt" or "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "png" => "image/png",
                "jpg" or "jpeg" => "image/jpeg",
                "gif" => "image/gif",
                "txt" => "text/plain",
                "zip" => "application/zip",
                "rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }
    }
}
