using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Calendar;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly ISuKienRepository _eventRepository;
        private readonly ILichHocRepository _classRepository;
        private readonly ILichThiRepository _examRepository;
        private readonly IGenericRepository<CongViec> _taskRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CalendarService> _logger;

        public CalendarService(
            ISuKienRepository eventRepository,
            ILichHocRepository classRepository,
            ILichThiRepository examRepository,
            IGenericRepository<CongViec> taskRepository,
            INotificationService notificationService,
            ILogger<CalendarService> logger)
        {
            _eventRepository = eventRepository;
            _classRepository = classRepository;
            _examRepository = examRepository;
            _taskRepository = taskRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(int userId, DateTime start, DateTime end, string[]? types = null)
        {
            var result = new List<CalendarEventDto>();
            var activeTypes = types != null && types.Length > 0 
                ? new HashSet<string>(types, StringComparer.OrdinalIgnoreCase) 
                : new HashSet<string> { "PersonalEvent", "ClassSchedule", "ExamSchedule", "TaskDeadline" };

            // 1. Personal Events
            if (activeTypes.Contains("PersonalEvent"))
            {
                var personalEvents = await _eventRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(e => e.MonHoc)
                    .Where(e => e.MaNguoiDung == userId && e.TrangThai == 1 &&
                                e.ThoiGianBatDau <= end && e.ThoiGianKetThuc >= start)
                    .ToListAsync();

                result.AddRange(personalEvents.Select(e => new CalendarEventDto
                {
                    Id = $"Personal_{e.MaSuKien}",
                    SourceId = e.MaSuKien,
                    Title = e.TieuDe,
                    Description = e.MoTa ?? string.Empty,
                    Start = e.ThoiGianBatDau,
                    End = e.ThoiGianKetThuc,
                    EventType = "PersonalEvent",
                    Color = string.IsNullOrEmpty(e.MauSac) ? "#4F46E5" : e.MauSac,
                    Location = e.DiaDiem ?? string.Empty,
                    ReminderMinutes = e.NhacTruoc,
                    Status = e.TrangThai,
                    IsEditable = true,
                    MaMonHoc = e.MaMonHoc,
                    TenMonHoc = e.MonHoc?.TenMonHoc,
                    GiangVien = e.GiangVien,
                    HinhThucThi = e.HinhThucThi
                }));
            }

            // 2. Class Schedule (LichHoc)
            if (activeTypes.Contains("ClassSchedule"))
            {
                var classSchedules = await _classRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(l => l.MonHoc)
                    .Where(l => l.MaNguoiDung == userId && l.NgayBatDau <= end && l.NgayKetThuc >= start)
                    .ToListAsync();

                result.AddRange(classSchedules.Select(l => new CalendarEventDto
                {
                    Id = $"Class_{l.MaLichHoc}",
                    SourceId = l.MaLichHoc,
                    Title = !string.IsNullOrWhiteSpace(l.TieuDe) ? l.TieuDe : (l.MonHoc?.TenMonHoc ?? "Lịch học"),
                    Description = l.GhiChu ?? string.Empty,
                    Start = l.NgayBatDau,
                    End = l.NgayKetThuc,
                    EventType = "ClassSchedule",
                    Color = string.IsNullOrEmpty(l.MauSac) ? "#0EA5E9" : l.MauSac,
                    Location = l.PhongHoc ?? string.Empty,
                    Status = 1,
                    IsEditable = true,
                    MaMonHoc = l.MaMonHoc,
                    TenMonHoc = l.MonHoc?.TenMonHoc,
                    GiangVien = !string.IsNullOrWhiteSpace(l.GiangVien) ? l.GiangVien : "Giảng viên bộ môn"
                }));
            }

            // 3. Exam Schedule (LichThi)
            if (activeTypes.Contains("ExamSchedule"))
            {
                var exams = await _examRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(t => t.MonHoc)
                    .Where(t => t.MaNguoiDung == userId && t.NgayThi >= start && t.NgayThi <= end)
                    .ToListAsync();

                result.AddRange(exams.Select(t => new CalendarEventDto
                {
                    Id = $"Exam_{t.MaLichThi}",
                    SourceId = t.MaLichThi,
                    Title = !string.IsNullOrWhiteSpace(t.TieuDe) ? t.TieuDe : (t.MonHoc?.TenMonHoc ?? "Lịch thi"),
                    Description = t.GhiChu ?? string.Empty,
                    Start = t.NgayThi,
                    End = t.ThoiLuong.HasValue ? t.NgayThi.AddMinutes(t.ThoiLuong.Value) : t.NgayThi.AddHours(2),
                    EventType = "ExamSchedule",
                    Color = string.IsNullOrEmpty(t.MauSac) ? "#EF4444" : t.MauSac,
                    Location = t.PhongThi ?? string.Empty,
                    Status = 1,
                    IsEditable = true,
                    MaMonHoc = t.MaMonHoc,
                    TenMonHoc = t.MonHoc?.TenMonHoc,
                    GiangVien = !string.IsNullOrWhiteSpace(t.GiangVien) ? t.GiangVien : "Giảng viên bộ môn",
                    HinhThucThi = t.HinhThucThi
                }));
            }

            // 4. Task Deadlines (CongViec)
            if (activeTypes.Contains("TaskDeadline"))
            {
                var tasks = await _taskRepository.GetQueryable()
                    .AsNoTracking()
                    .Where(t => t.MaNguoiDung == userId && t.HanHoanThanh.HasValue &&
                                t.HanHoanThanh.Value >= start && t.HanHoanThanh.Value <= end && !t.DaXoa)
                    .ToListAsync();

                result.AddRange(tasks.Select(t => new CalendarEventDto
                {
                    Id = $"Task_{t.MaCongViec}",
                    SourceId = t.MaCongViec,
                    Title = $"Deadline: {t.TieuDe}",
                    Description = t.MoTa ?? string.Empty,
                    Start = t.HanHoanThanh!.Value,
                    End = t.HanHoanThanh!.Value.AddHours(1),
                    EventType = "TaskDeadline",
                    Color = "#F59E0B", // Amber
                    Status = t.TrangThai,
                    IsEditable = false
                }));
            }

            return result.OrderBy(e => e.Start);
        }

        public async Task<CalendarEventDto> CreateEventAsync(int userId, CreateCalendarEventRequest request)
        {
            string eventType = request.EventType ?? (request.TieuDe.ToLower().Contains("thi") ? "ExamSchedule" : (request.TieuDe.ToLower().Contains("học") ? "ClassSchedule" : "PersonalEvent"));

            if (eventType == "ClassSchedule")
            {
                int monHocId = request.MaMonHoc ?? 0;
                string? monHocName = null;
                if (monHocId > 0)
                {
                    monHocName = await _classRepository.GetQueryable().Where(l => l.MaMonHoc == monHocId).Select(l => l.MonHoc.TenMonHoc).FirstOrDefaultAsync();
                }
                if (monHocId == 0)
                {
                    monHocId = await _classRepository.GetQueryable().Select(l => l.MaMonHoc).FirstOrDefaultAsync();
                    if (monHocId == 0) monHocId = await _examRepository.GetQueryable().Select(t => t.MaMonHoc).FirstOrDefaultAsync();
                    if (monHocId == 0) monHocId = 2;
                }

                var lichHoc = new LichHoc
                {
                    MaNguoiDung = userId,
                    MaMonHoc = monHocId,
                    TieuDe = request.TieuDe.Trim(),
                    Thu = (byte)(request.ThoiGianBatDau.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)request.ThoiGianBatDau.DayOfWeek + 1),
                    TietBatDau = (byte)request.ThoiGianBatDau.Hour,
                    TietKetThuc = (byte)request.ThoiGianKetThuc.Hour,
                    PhongHoc = request.DiaDiem ?? "Phòng học",
                    GiangVien = !string.IsNullOrWhiteSpace(request.GiangVien) ? request.GiangVien.Trim() : "Giảng viên bộ môn",
                    NgayBatDau = request.ThoiGianBatDau,
                    NgayKetThuc = request.ThoiGianKetThuc,
                    MauSac = request.MauSac ?? "#0EA5E9",
                    GhiChu = request.MoTa ?? string.Empty
                };

                await _classRepository.AddAsync(lichHoc);
                await _classRepository.SaveAsync();

                try
                {
                    await _notificationService.CreateNotificationAsync(new Application.DTOs.Notification.CreateNotificationRequest
                    {
                        MaNguoiDung = userId,
                        MaLoaiThongBao = 2, // Lịch học
                        TieuDe = $"Đã tạo lịch học mới: {lichHoc.TieuDe}",
                        NoiDung = $"Lịch học môn \"{lichHoc.TieuDe}\" vào lúc {lichHoc.NgayBatDau:HH:mm dd/MM/yyyy} đã được tạo thành công.",
                        DuongDan = "/calendar",
                        MucDo = 1
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not send notification for class schedule creation.");
                }

                return new CalendarEventDto
                {
                    Id = $"Class_{lichHoc.MaLichHoc}",
                    SourceId = lichHoc.MaLichHoc,
                    Title = lichHoc.TieuDe,
                    Description = lichHoc.GhiChu,
                    Start = lichHoc.NgayBatDau,
                    End = lichHoc.NgayKetThuc,
                    EventType = "ClassSchedule",
                    Color = lichHoc.MauSac,
                    Location = lichHoc.PhongHoc,
                    Status = 1,
                    IsEditable = true,
                    MaMonHoc = lichHoc.MaMonHoc,
                    GiangVien = lichHoc.GiangVien
                };
            }
            else if (eventType == "ExamSchedule")
            {
                int monHocId = request.MaMonHoc ?? 0;
                string? monHocName = null;
                if (monHocId > 0)
                {
                    monHocName = await _examRepository.GetQueryable().Where(t => t.MaMonHoc == monHocId).Select(t => t.MonHoc.TenMonHoc).FirstOrDefaultAsync();
                }
                if (monHocId == 0)
                {
                    monHocId = await _examRepository.GetQueryable().Select(t => t.MaMonHoc).FirstOrDefaultAsync();
                    if (monHocId == 0) monHocId = await _classRepository.GetQueryable().Select(l => l.MaMonHoc).FirstOrDefaultAsync();
                    if (monHocId == 0) monHocId = 2;
                }

                var thoiLuongMinutes = (int)(request.ThoiGianKetThuc - request.ThoiGianBatDau).TotalMinutes;
                if (thoiLuongMinutes <= 0) thoiLuongMinutes = 90;

                var lichThi = new LichThi
                {
                    MaNguoiDung = userId,
                    MaMonHoc = monHocId,
                    TieuDe = request.TieuDe.Trim(),
                    GiangVien = !string.IsNullOrWhiteSpace(request.GiangVien) ? request.GiangVien.Trim() : "Giảng viên bộ môn",
                    HinhThucThi = !string.IsNullOrWhiteSpace(request.HinhThucThi) ? request.HinhThucThi.Trim() : "Thi tự luận / Trắc nghiệm",
                    NgayThi = request.ThoiGianBatDau,
                    ThoiLuong = thoiLuongMinutes,
                    PhongThi = request.DiaDiem ?? "Phòng thi A101",
                    MauSac = request.MauSac ?? "#EF4444",
                    GhiChu = request.MoTa ?? string.Empty
                };

                await _examRepository.AddAsync(lichThi);
                await _examRepository.SaveAsync();

                try
                {
                    await _notificationService.CreateNotificationAsync(new Application.DTOs.Notification.CreateNotificationRequest
                    {
                        MaNguoiDung = userId,
                        MaLoaiThongBao = 2, // Lịch thi
                        TieuDe = $"Đã tạo lịch thi mới: {lichThi.TieuDe}",
                        NoiDung = $"Kỳ thi \"{lichThi.TieuDe}\" ({lichThi.HinhThucThi}) diễn ra vào lúc {lichThi.NgayThi:HH:mm dd/MM/yyyy} đã được thêm vào lịch.",
                        DuongDan = "/calendar",
                        MucDo = 2
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not send notification for exam creation.");
                }

                return new CalendarEventDto
                {
                    Id = $"Exam_{lichThi.MaLichThi}",
                    SourceId = lichThi.MaLichThi,
                    Title = lichThi.TieuDe,
                    Description = lichThi.GhiChu,
                    Start = lichThi.NgayThi,
                    End = request.ThoiGianKetThuc,
                    EventType = "ExamSchedule",
                    Color = string.IsNullOrEmpty(lichThi.MauSac) ? "#EF4444" : lichThi.MauSac,
                    Location = lichThi.PhongThi,
                    Status = 1,
                    IsEditable = true,
                    MaMonHoc = lichThi.MaMonHoc,
                    GiangVien = lichThi.GiangVien,
                    HinhThucThi = lichThi.HinhThucThi
                };
            }
            else
            {
                var suKien = new SuKien
                {
                    MaNguoiDung = userId,
                    MaMonHoc = request.MaMonHoc,
                    TieuDe = request.TieuDe.Trim(),
                    MoTa = request.MoTa ?? string.Empty,
                    GiangVien = request.GiangVien,
                    HinhThucThi = request.HinhThucThi,
                    ThoiGianBatDau = request.ThoiGianBatDau,
                    ThoiGianKetThuc = request.ThoiGianKetThuc,
                    DiaDiem = request.DiaDiem ?? string.Empty,
                    MauSac = request.MauSac ?? "#4F46E5",
                    NhacTruoc = request.NhacTruoc,
                    TrangThai = 1
                };

                await _eventRepository.AddAsync(suKien);
                await _eventRepository.SaveAsync();

                try
                {
                    await _notificationService.CreateNotificationAsync(new Application.DTOs.Notification.CreateNotificationRequest
                    {
                        MaNguoiDung = userId,
                        MaLoaiThongBao = 2, // Lịch học/thi/sự kiện
                        TieuDe = $"Đã thêm sự kiện mới: {suKien.TieuDe}",
                        NoiDung = $"Sự kiện \"{suKien.TieuDe}\" diễn ra vào lúc {suKien.ThoiGianBatDau:HH:mm dd/MM/yyyy} đã được lên lịch.",
                        DuongDan = "/calendar",
                        MucDo = 1
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not send notification for personal event creation.");
                }

                _logger.LogInformation("Người dùng {UserId} tạo sự kiện cá nhân mới {EventId}: {Title}", userId, suKien.MaSuKien, suKien.TieuDe);

                return new CalendarEventDto
                {
                    Id = $"Personal_{suKien.MaSuKien}",
                    SourceId = suKien.MaSuKien,
                    Title = suKien.TieuDe,
                    Description = suKien.MoTa ?? string.Empty,
                    Start = suKien.ThoiGianBatDau,
                    End = suKien.ThoiGianKetThuc,
                    EventType = "PersonalEvent",
                    Color = suKien.MauSac,
                    Location = suKien.DiaDiem ?? string.Empty,
                    ReminderMinutes = suKien.NhacTruoc,
                    Status = suKien.TrangThai,
                    IsEditable = true,
                    MaMonHoc = suKien.MaMonHoc,
                    GiangVien = suKien.GiangVien,
                    HinhThucThi = suKien.HinhThucThi
                };
            }
        }

        public async Task<CalendarEventDto> UpdateEventAsync(int eventId, int userId, UpdateCalendarEventRequest request)
        {
            // 1. Try SuKien
            var suKien = await _eventRepository.GetQueryable()
                .FirstOrDefaultAsync(e => e.MaSuKien == eventId && e.MaNguoiDung == userId);
            if (suKien != null)
            {
                suKien.TieuDe = request.TieuDe.Trim();
                suKien.MoTa = request.MoTa ?? string.Empty;
                if (request.MaMonHoc.HasValue && request.MaMonHoc > 0) suKien.MaMonHoc = request.MaMonHoc;
                if (!string.IsNullOrWhiteSpace(request.GiangVien)) suKien.GiangVien = request.GiangVien.Trim();
                if (!string.IsNullOrWhiteSpace(request.HinhThucThi)) suKien.HinhThucThi = request.HinhThucThi.Trim();
                suKien.ThoiGianBatDau = request.ThoiGianBatDau;
                suKien.ThoiGianKetThuc = request.ThoiGianKetThuc;
                suKien.DiaDiem = request.DiaDiem ?? string.Empty;
                if (!string.IsNullOrEmpty(request.MauSac)) suKien.MauSac = request.MauSac;
                suKien.NhacTruoc = request.NhacTruoc;
                suKien.TrangThai = request.TrangThai;

                _eventRepository.Update(suKien);
                await _eventRepository.SaveAsync();

                _logger.LogInformation("Người dùng {UserId} cập nhật sự kiện cá nhân {EventId}", userId, eventId);

                return new CalendarEventDto
                {
                    Id = $"Personal_{suKien.MaSuKien}",
                    SourceId = suKien.MaSuKien,
                    Title = suKien.TieuDe,
                    Description = suKien.MoTa ?? string.Empty,
                    Start = suKien.ThoiGianBatDau,
                    End = suKien.ThoiGianKetThuc,
                    EventType = "PersonalEvent",
                    Color = suKien.MauSac,
                    Location = suKien.DiaDiem ?? string.Empty,
                    ReminderMinutes = suKien.NhacTruoc,
                    Status = suKien.TrangThai,
                    IsEditable = true,
                    MaMonHoc = suKien.MaMonHoc,
                    GiangVien = suKien.GiangVien,
                    HinhThucThi = suKien.HinhThucThi
                };
            }

            // 2. Try LichHoc
            var lichHoc = await _classRepository.GetQueryable()
                .FirstOrDefaultAsync(l => l.MaLichHoc == eventId && l.MaNguoiDung == userId);
            if (lichHoc != null)
            {
                lichHoc.TieuDe = request.TieuDe.Trim();
                lichHoc.NgayBatDau = request.ThoiGianBatDau;
                lichHoc.NgayKetThuc = request.ThoiGianKetThuc;
                lichHoc.PhongHoc = request.DiaDiem ?? lichHoc.PhongHoc;
                if (!string.IsNullOrEmpty(request.MauSac)) lichHoc.MauSac = request.MauSac;
                lichHoc.GhiChu = request.MoTa ?? lichHoc.GhiChu;
                if (request.MaMonHoc.HasValue && request.MaMonHoc > 0) lichHoc.MaMonHoc = request.MaMonHoc.Value;
                if (!string.IsNullOrWhiteSpace(request.GiangVien)) lichHoc.GiangVien = request.GiangVien.Trim();

                _classRepository.Update(lichHoc);
                await _classRepository.SaveAsync();

                return new CalendarEventDto
                {
                    Id = $"Class_{lichHoc.MaLichHoc}",
                    SourceId = lichHoc.MaLichHoc,
                    Title = lichHoc.TieuDe,
                    Description = lichHoc.GhiChu,
                    Start = lichHoc.NgayBatDau,
                    End = lichHoc.NgayKetThuc,
                    EventType = "ClassSchedule",
                    Color = lichHoc.MauSac,
                    Location = lichHoc.PhongHoc,
                    Status = 1,
                    IsEditable = true,
                    MaMonHoc = lichHoc.MaMonHoc,
                    GiangVien = lichHoc.GiangVien
                };
            }

            // 3. Try LichThi
            var lichThi = await _examRepository.GetQueryable()
                .FirstOrDefaultAsync(t => t.MaLichThi == eventId && t.MaNguoiDung == userId);
            if (lichThi != null)
            {
                lichThi.TieuDe = request.TieuDe.Trim();
                lichThi.NgayThi = request.ThoiGianBatDau;
                var duration = (int)(request.ThoiGianKetThuc - request.ThoiGianBatDau).TotalMinutes;
                if (duration > 0) lichThi.ThoiLuong = duration;
                lichThi.PhongThi = request.DiaDiem ?? lichThi.PhongThi;
                lichThi.GhiChu = request.MoTa ?? lichThi.GhiChu;
                if (!string.IsNullOrEmpty(request.MauSac)) lichThi.MauSac = request.MauSac;
                if (request.MaMonHoc.HasValue && request.MaMonHoc > 0) lichThi.MaMonHoc = request.MaMonHoc.Value;
                if (!string.IsNullOrWhiteSpace(request.GiangVien)) lichThi.GiangVien = request.GiangVien.Trim();
                if (!string.IsNullOrWhiteSpace(request.HinhThucThi)) lichThi.HinhThucThi = request.HinhThucThi.Trim();

                _examRepository.Update(lichThi);
                await _examRepository.SaveAsync();

                return new CalendarEventDto
                {
                    Id = $"Exam_{lichThi.MaLichThi}",
                    SourceId = lichThi.MaLichThi,
                    Title = lichThi.TieuDe,
                    Description = lichThi.GhiChu,
                    Start = lichThi.NgayThi,
                    End = request.ThoiGianKetThuc,
                    EventType = "ExamSchedule",
                    Color = string.IsNullOrEmpty(lichThi.MauSac) ? "#EF4444" : lichThi.MauSac,
                    Location = lichThi.PhongThi,
                    Status = 1,
                    IsEditable = true,
                    MaMonHoc = lichThi.MaMonHoc,
                    GiangVien = lichThi.GiangVien,
                    HinhThucThi = lichThi.HinhThucThi
                };
            }

            throw new NotFoundException($"Sự kiện ID {eventId} không tồn tại trong CSDL.");
        }

        public async Task DeleteEventAsync(int eventId, int userId, string? eventType = null)
        {
            if (!string.IsNullOrEmpty(eventType))
            {
                if (eventType.Equals("ClassSchedule", StringComparison.OrdinalIgnoreCase) || eventType.Equals("Class", StringComparison.OrdinalIgnoreCase))
                {
                    var lichHoc = await _classRepository.GetQueryable()
                        .FirstOrDefaultAsync(l => l.MaLichHoc == eventId && l.MaNguoiDung == userId);
                    if (lichHoc != null)
                    {
                        _classRepository.Delete(lichHoc);
                        await _classRepository.SaveAsync();
                        _logger.LogInformation("Người dùng {UserId} đã xóa lịch học {EventId}", userId, eventId);
                        return;
                    }
                }
                else if (eventType.Equals("ExamSchedule", StringComparison.OrdinalIgnoreCase) || eventType.Equals("Exam", StringComparison.OrdinalIgnoreCase))
                {
                    var lichThi = await _examRepository.GetQueryable()
                        .FirstOrDefaultAsync(t => t.MaLichThi == eventId && t.MaNguoiDung == userId);
                    if (lichThi != null)
                    {
                        _examRepository.Delete(lichThi);
                        await _examRepository.SaveAsync();
                        _logger.LogInformation("Người dùng {UserId} đã xóa lịch thi {EventId}", userId, eventId);
                        return;
                    }
                }
                else if (eventType.Equals("PersonalEvent", StringComparison.OrdinalIgnoreCase) || eventType.Equals("Personal", StringComparison.OrdinalIgnoreCase))
                {
                    var suKien = await _eventRepository.GetQueryable()
                        .FirstOrDefaultAsync(e => e.MaSuKien == eventId && e.MaNguoiDung == userId);
                    if (suKien != null)
                    {
                        _eventRepository.Delete(suKien);
                        await _eventRepository.SaveAsync();
                        _logger.LogInformation("Người dùng {UserId} đã xóa sự kiện cá nhân {EventId}", userId, eventId);
                        return;
                    }
                }
            }

            // Fallback: search in all 3 tables sequentially if eventType is not matched or null
            var sk = await _eventRepository.GetQueryable()
                .FirstOrDefaultAsync(e => e.MaSuKien == eventId && e.MaNguoiDung == userId);
            if (sk != null)
            {
                _eventRepository.Delete(sk);
                await _eventRepository.SaveAsync();
                _logger.LogInformation("Người dùng {UserId} đã xóa sự kiện cá nhân {EventId}", userId, eventId);
                return;
            }

            var lh = await _classRepository.GetQueryable()
                .FirstOrDefaultAsync(l => l.MaLichHoc == eventId && l.MaNguoiDung == userId);
            if (lh != null)
            {
                _classRepository.Delete(lh);
                await _classRepository.SaveAsync();
                _logger.LogInformation("Người dùng {UserId} đã xóa lịch học {EventId}", userId, eventId);
                return;
            }

            var lt = await _examRepository.GetQueryable()
                .FirstOrDefaultAsync(t => t.MaLichThi == eventId && t.MaNguoiDung == userId);
            if (lt != null)
            {
                _examRepository.Delete(lt);
                await _examRepository.SaveAsync();
                _logger.LogInformation("Người dùng {UserId} đã xóa lịch thi {EventId}", userId, eventId);
                return;
            }

            throw new NotFoundException($"Không tìm thấy bản ghi ID {eventId} để xóa trong CSDL.");
        }
    }
}
