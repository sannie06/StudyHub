using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Pomodoro;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class PomodoroService : IPomodoroService
    {
        private readonly IGenericRepository<PomodoroSession> _pomodoroRepository;
        private readonly IGenericRepository<CongViec> _taskRepository;
        private readonly IGenericRepository<CauHinhHeThong> _configRepository;
        private readonly IGenericRepository<ThongKeHocTap> _statsRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PomodoroService> _logger;

        public PomodoroService(
            IGenericRepository<PomodoroSession> pomodoroRepository,
            IGenericRepository<CongViec> taskRepository,
            IGenericRepository<CauHinhHeThong> configRepository,
            IGenericRepository<ThongKeHocTap> statsRepository,
            INotificationService notificationService,
            ILogger<PomodoroService> logger)
        {
            _pomodoroRepository = pomodoroRepository;
            _taskRepository = taskRepository;
            _configRepository = configRepository;
            _statsRepository = statsRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<PomodoroSessionDto> StartSessionAsync(int userId, StartPomodoroRequest request)
        {
            // Cancel any existing active/running session of the user first
            var activeSession = await _pomodoroRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.MaNguoiDung == userId && s.TrangThai == 2);

            if (activeSession != null)
            {
                activeSession.TrangThai = 0; // Cancel
                activeSession.ThoiGianKetThuc = DateTime.UtcNow;
                _pomodoroRepository.Update(activeSession);
                _logger.LogInformation("Cancelled previous active Pomodoro Session {SessionId} for User {UserId}.", activeSession.MaSession, userId);
            }

            var session = new PomodoroSession
            {
                MaNguoiDung = userId,
                MaCongViec = request.MaCongViec,
                MaMonHoc = request.MaMonHoc,
                TieuDe = request.TieuDe ?? (request.LoaiSession == 0 ? "Focus Session" : "Break Session"),
                LoaiSession = request.LoaiSession,
                ThoiLuong = request.ThoiLuong,
                SoLanTamDung = 0,
                TongThoiGianTamDung = 0,
                ThoiGianBatDau = DateTime.UtcNow,
                TrangThai = 2, // Running
                NgayTao = DateTime.UtcNow
            };

            await _pomodoroRepository.AddAsync(session);
            await _pomodoroRepository.SaveAsync();

            _logger.LogInformation("Started new Pomodoro Session {SessionId} (Type: {Type}) for User {UserId}.", session.MaSession, session.LoaiSession, userId);

            return MapToDto(session);
        }

        public async Task<PomodoroSessionDto> PauseSessionAsync(int id, int userId, PausePomodoroRequest request)
        {
            var session = await _pomodoroRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.MaSession == id && s.MaNguoiDung == userId && s.TrangThai == 2);

            if (session == null)
            {
                throw new NotFoundException("Không tìm thấy phiên Pomodoro đang hoạt động.");
            }

            session.SoLanTamDung += 1;
            session.TongThoiGianTamDung = request.TongThoiGianTamDung;

            _pomodoroRepository.Update(session);
            await _pomodoroRepository.SaveAsync();

            _logger.LogInformation("Paused Pomodoro Session {SessionId} for User {UserId}. Pause count: {Count}, Total paused: {Duration}s.", id, userId, session.SoLanTamDung, session.TongThoiGianTamDung);

            return MapToDto(session);
        }

        public async Task<PomodoroSessionDto> FinishSessionAsync(int id, int userId, FinishPomodoroRequest request)
        {
            var session = await _pomodoroRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.MaSession == id && s.MaNguoiDung == userId && s.TrangThai == 2);

            if (session == null)
            {
                throw new NotFoundException("Không tìm thấy phiên Pomodoro đang hoạt động.");
            }

            session.TrangThai = 1; // Completed
            session.ThoiGianKetThuc = DateTime.UtcNow;
            session.TongThoiGianTamDung = request.TongThoiGianTamDung;
            session.SoLanTamDung = request.SoLanTamDung;

            _pomodoroRepository.Update(session);

            // Auto-update task progress if Focus session is completed and linked to a task
            if (session.LoaiSession == 0 && session.MaCongViec.HasValue)
            {
                var task = await _taskRepository.GetByIdAsync(session.MaCongViec.Value);
                if (task != null && task.MaNguoiDung == userId)
                {
                    // Retrieve increment from configuration or default to 20%
                    var increment = 20;
                    var config = await _configRepository.GetQueryable()
                        .FirstOrDefaultAsync(c => c.TenCauHinh == "PomodoroProgressIncrement");
                    if (config != null && int.TryParse(config.GiaTri, out var parsedValue))
                    {
                        increment = parsedValue;
                    }

                    var newProgress = task.TiLeHoanThanh + increment;
                    if (newProgress >= 100)
                    {
                        task.TiLeHoanThanh = 100;
                        task.TrangThai = 3; // Completed
                        task.NgayHoanThanh = DateTime.UtcNow;
                    }
                    else
                    {
                        task.TiLeHoanThanh = (byte)newProgress;
                    }
                    task.NgayCapNhat = DateTime.UtcNow;
                    _taskRepository.Update(task);

                    _logger.LogInformation("Auto-updated progress of Task {TaskId} to {Progress}% after finishing Pomodoro Session {SessionId}.", task.MaCongViec, task.TiLeHoanThanh, id);
                }
            }

            // Real-time pre-calculated statistics update
            if (session.LoaiSession == 0) // Focus Session
            {
                await UpdateDailyStatisticsAsync(userId, session.ThoiLuong);
            }

            await _pomodoroRepository.SaveAsync();

            // Trigger Realtime Pomodoro Completion Notification
            try
            {
                await _notificationService.CreateNotificationAsync(new StudyHub.Application.DTOs.Notification.CreateNotificationRequest
                {
                    MaNguoiDung = userId,
                    MaLoaiThongBao = 5,
                    TieuDe = "Cột mốc hoàn thành Pomodoro",
                    NoiDung = $"Bạn vừa hoàn thành {session.ThoiLuong} phút tập trung Pomodoro! Hãy nghỉ ngơi 5 phút.",
                    DuongDan = "/pomodoro",
                    MucDo = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể phát thông báo hoàn thành Pomodoro cho người dùng {UserId}", userId);
            }

            _logger.LogInformation("Finished Pomodoro Session {SessionId} successfully for User {UserId}.", id, userId);

            return MapToDto(session);
        }

        public async Task<PomodoroSessionDto> CancelSessionAsync(int id, int userId)
        {
            var session = await _pomodoroRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.MaSession == id && s.MaNguoiDung == userId && s.TrangThai == 2);

            if (session == null)
            {
                throw new NotFoundException("Không tìm thấy phiên Pomodoro đang hoạt động.");
            }

            session.TrangThai = 0; // Cancelled
            session.ThoiGianKetThuc = DateTime.UtcNow;

            _pomodoroRepository.Update(session);
            await _pomodoroRepository.SaveAsync();

            _logger.LogInformation("Cancelled Pomodoro Session {SessionId} for User {UserId}.", id, userId);

            return MapToDto(session);
        }

        public async Task<PomodoroSessionDto> GetActiveSessionAsync(int userId)
        {
            var session = await _pomodoroRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.MaNguoiDung == userId && s.TrangThai == 2);

            if (session == null)
            {
                return null;
            }

            return MapToDto(session);
        }

        private async Task UpdateDailyStatisticsAsync(int userId, int focusMinutes)
        {
            var today = DateTime.UtcNow.Date;
            var stats = await _statsRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.MaNguoiDung == userId && s.NgayThongKe.Date == today);

            if (stats == null)
            {
                // Retrieve yesterday's streak
                var yesterday = today.AddDays(-1);
                var yesterdayStats = await _statsRepository.GetQueryable()
                    .FirstOrDefaultAsync(s => s.MaNguoiDung == userId && s.NgayThongKe.Date == yesterday);

                var newStreak = (yesterdayStats?.SoNgayHocLienTiep ?? 0) + 1;

                stats = new ThongKeHocTap
                {
                    MaNguoiDung = userId,
                    TongCongViec = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId),
                    CongViecHoanThanh = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId && t.TrangThai == 3),
                    CongViecQuaHan = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId && t.TrangThai == 4),
                    TongPomodoro = 1,
                    TongPhutHoc = focusMinutes,
                    SoNgayHocLienTiep = newStreak,
                    NgayThongKe = today
                };

                // Calculate completed percentage
                if (stats.TongCongViec > 0)
                {
                    stats.TyLeHoanThanh = (decimal)stats.CongViecHoanThanh / stats.TongCongViec * 100;
                }
                
                // Productivity score: based on focus minutes and completed tasks
                stats.DiemNangSuat = stats.TongPhutHoc + (stats.CongViecHoanThanh * 10);

                await _statsRepository.AddAsync(stats);
            }
            else
            {
                stats.TongPomodoro += 1;
                stats.TongPhutHoc += focusMinutes;
                stats.TongCongViec = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId);
                stats.CongViecHoanThanh = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId && t.TrangThai == 3);
                stats.CongViecQuaHan = await _taskRepository.GetQueryable().CountAsync(t => t.MaNguoiDung == userId && t.TrangThai == 4);

                if (stats.TongCongViec > 0)
                {
                    stats.TyLeHoanThanh = (decimal)stats.CongViecHoanThanh / stats.TongCongViec * 100;
                }
                stats.DiemNangSuat = stats.TongPhutHoc + (stats.CongViecHoanThanh * 10);

                _statsRepository.Update(stats);
            }
        }

        private PomodoroSessionDto MapToDto(PomodoroSession session)
        {
            return new PomodoroSessionDto
            {
                MaSession = session.MaSession,
                MaNguoiDung = session.MaNguoiDung,
                MaCongViec = session.MaCongViec,
                MaMonHoc = session.MaMonHoc,
                TieuDe = session.TieuDe,
                LoaiSession = session.LoaiSession,
                ThoiLuong = session.ThoiLuong,
                SoLanTamDung = session.SoLanTamDung,
                TongThoiGianTamDung = session.TongThoiGianTamDung,
                ThoiGianBatDau = session.ThoiGianBatDau,
                ThoiGianKetThuc = session.ThoiGianKetThuc,
                TrangThai = session.TrangThai
            };
        }
    }
}
