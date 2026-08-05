using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Common;
using StudyHub.Application.DTOs.Task;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly IGenericRepository<CongViec> _taskRepository;
        private readonly IGenericRepository<MonHoc> _subjectRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            IGenericRepository<CongViec> taskRepository,
            IGenericRepository<MonHoc> subjectRepository,
            INotificationService notificationService,
            ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _subjectRepository = subjectRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<PagedList<TaskDto>> GetTasksAsync(int userId, TaskQueryParameters queryParameters)
        {
            var query = _taskRepository.GetQueryable().Where(t => t.MaNguoiDung == userId);

            // Filters
            if (!string.IsNullOrEmpty(queryParameters.Search))
            {
                var search = queryParameters.Search.ToLower();
                query = query.Where(t => t.TieuDe.ToLower().Contains(search) || 
                                         (t.MoTa != null && t.MoTa.ToLower().Contains(search)));
            }

            if (queryParameters.Priority.HasValue)
            {
                query = query.Where(t => t.DoUuTien == queryParameters.Priority.Value);
            }

            if (queryParameters.Status.HasValue)
            {
                query = query.Where(t => t.TrangThai == queryParameters.Status.Value);
            }

            if (queryParameters.SubjectId.HasValue)
            {
                query = query.Where(t => t.MaMonHoc == queryParameters.SubjectId.Value);
            }

            // Sorting
            bool isDesc = string.Equals(queryParameters.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                var sortBy = queryParameters.SortBy.ToLower();
                if (sortBy == "duedate" || sortBy == "hanhoanthanh")
                {
                    query = isDesc ? query.OrderByDescending(t => t.HanHoanThanh) : query.OrderBy(t => t.HanHoanThanh);
                }
                else if (sortBy == "priority" || sortBy == "douutien")
                {
                    query = isDesc ? query.OrderByDescending(t => t.DoUuTien) : query.OrderBy(t => t.DoUuTien);
                }
                else if (sortBy == "title" || sortBy == "tieude")
                {
                    query = isDesc ? query.OrderByDescending(t => t.TieuDe) : query.OrderBy(t => t.TieuDe);
                }
                else
                {
                    query = isDesc ? query.OrderByDescending(t => t.MaCongViec) : query.OrderBy(t => t.MaCongViec);
                }
            }
            else
            {
                query = query.OrderByDescending(t => t.MaCongViec);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .Select(t => new TaskDto
                {
                    MaCongViec = t.MaCongViec,
                    MaNguoiDung = t.MaNguoiDung,
                    MaMonHoc = t.MaMonHoc,
                    TenMonHoc = t.MonHoc != null ? t.MonHoc.TenMonHoc : null,
                    MaMon = t.MonHoc != null ? t.MonHoc.MaMon : null,
                    TieuDe = t.TieuDe,
                    MoTa = t.MoTa,
                    DoUuTien = t.DoUuTien,
                    TrangThai = t.TrangThai,
                    NgayBatDau = t.NgayBatDau,
                    HanHoanThanh = t.HanHoanThanh,
                    NgayHoanThanh = t.NgayHoanThanh,
                    TiLeHoanThanh = t.TiLeHoanThanh,
                    MauSac = t.MauSac,
                    DanhDauQuanTrong = t.DanhDauQuanTrong,
                    DanhDauYeuThich = t.DanhDauYeuThich,
                    GhiChu = t.GhiChu
                })
                .ToListAsync();

            return new PagedList<TaskDto>(items, totalCount, queryParameters.PageNumber, queryParameters.PageSize);
        }

        public async Task<TaskDto> GetTaskByIdAsync(int id, int userId)
        {
            var task = await _taskRepository.GetQueryable()
                .Where(t => t.MaCongViec == id && t.MaNguoiDung == userId)
                .Select(t => new TaskDto
                {
                    MaCongViec = t.MaCongViec,
                    MaNguoiDung = t.MaNguoiDung,
                    MaMonHoc = t.MaMonHoc,
                    TenMonHoc = t.MonHoc != null ? t.MonHoc.TenMonHoc : null,
                    MaMon = t.MonHoc != null ? t.MonHoc.MaMon : null,
                    TieuDe = t.TieuDe,
                    MoTa = t.MoTa,
                    DoUuTien = t.DoUuTien,
                    TrangThai = t.TrangThai,
                    NgayBatDau = t.NgayBatDau,
                    HanHoanThanh = t.HanHoanThanh,
                    NgayHoanThanh = t.NgayHoanThanh,
                    TiLeHoanThanh = t.TiLeHoanThanh,
                    MauSac = t.MauSac,
                    DanhDauQuanTrong = t.DanhDauQuanTrong,
                    DanhDauYeuThich = t.DanhDauYeuThich,
                    GhiChu = t.GhiChu
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                throw new NotFoundException("Công việc không tồn tại.");
            }

            return task;
        }

        public async Task<TaskDto> CreateTaskAsync(int userId, CreateTaskRequest request)
        {
            if (request.MaMonHoc.HasValue)
            {
                try
                {
                    var subject = await _subjectRepository.GetQueryable()
                        .FirstOrDefaultAsync(s => s.MaMonHoc == request.MaMonHoc.Value);
                    if (subject == null)
                    {
                        _logger.LogWarning("Create task warning: Selected subject {SubjectId} does not exist for User {UserId}.", request.MaMonHoc.Value, userId);
                        request.MaMonHoc = null; // Clear invalid MaMonHoc fallback
                    }
                }
                catch
                {
                    request.MaMonHoc = null;
                }
            }

            // Auto-resolve or create MonHoc record if TenMonHoc is provided and MaMonHoc is not set
            if (!request.MaMonHoc.HasValue && !string.IsNullOrWhiteSpace(request.TenMonHoc))
            {
                var targetName = request.TenMonHoc.Trim();
                var existingSubject = await _subjectRepository.GetQueryable()
                    .FirstOrDefaultAsync(s => s.TenMonHoc.ToLower() == targetName.ToLower());

                if (existingSubject != null)
                {
                    request.MaMonHoc = existingSubject.MaMonHoc;
                }
                else
                {
                    try
                    {
                        var newSubject = new MonHoc
                        {
                            TenMonHoc = targetName,
                            MaMon = targetName.Length >= 3 ? targetName.Substring(0, 3).ToUpper() : targetName.ToUpper(),
                            MoTa = "Thẻ Môn học tự động tạo",
                            MauSac = !string.IsNullOrWhiteSpace(request.MauSac) ? request.MauSac : "#6366F1",
                            Icon = "book",
                            TrangThai = 1
                        };
                        await _subjectRepository.AddAsync(newSubject);
                        await _subjectRepository.SaveAsync();
                        request.MaMonHoc = newSubject.MaMonHoc;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Could not create MonHoc '{TenMonHoc}': {Message}. Proceeding with MaMonHoc = null.", targetName, ex.Message);
                        request.MaMonHoc = null;
                    }
                }
            }

            var task = new CongViec
            {
                MaNguoiDung = userId,
                MaMonHoc = request.MaMonHoc,
                TieuDe = request.TieuDe ?? string.Empty,
                MoTa = request.MoTa ?? string.Empty,
                DoUuTien = request.DoUuTien,
                TrangThai = request.TrangThai,
                NgayBatDau = request.NgayBatDau,
                HanHoanThanh = request.HanHoanThanh,
                TiLeHoanThanh = request.TiLeHoanThanh,
                MauSac = request.MauSac ?? string.Empty,
                DanhDauQuanTrong = request.DanhDauQuanTrong,
                DanhDauYeuThich = request.DanhDauYeuThich,
                GhiChu = request.GhiChu ?? string.Empty,
                NgayTao = DateTime.UtcNow
            };

            if (request.TrangThai == 3)
            {
                task.NgayHoanThanh = DateTime.UtcNow;
            }

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveAsync();

            // Trigger Realtime Notification
            try
            {
                await _notificationService.CreateNotificationAsync(new StudyHub.Application.DTOs.Notification.CreateNotificationRequest
                {
                    MaNguoiDung = userId,
                    MaLoaiThongBao = 1, // Công việc
                    TieuDe = "Công việc mới đã tạo",
                    NoiDung = $"Bạn vừa tạo thành công công việc: \"{task.TieuDe}\"",
                    DuongDan = "/tasks",
                    MucDo = (byte)(task.DoUuTien == 2 ? 2 : 1)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể phát thông báo tạo công việc cá nhân cho người dùng {UserId}", userId);
            }

            _logger.LogInformation("Task created successfully: Title '{Title}' (ID: {TaskId}) for User {UserId}.", task.TieuDe, task.MaCongViec, userId);

            return await GetTaskByIdAsync(task.MaCongViec, userId);
        }

        public async Task<TaskDto> UpdateTaskAsync(int id, int userId, UpdateTaskRequest request)
        {
            var task = await _taskRepository.GetQueryable()
                .FirstOrDefaultAsync(t => t.MaCongViec == id && t.MaNguoiDung == userId);

            if (task == null)
            {
                _logger.LogWarning("Update task failed: Task {TaskId} not found for User {UserId}.", id, userId);
                throw new NotFoundException("Công việc không tồn tại.");
            }

            if (request.MaMonHoc.HasValue)
            {
                try
                {
                    var subject = await _subjectRepository.GetQueryable()
                        .FirstOrDefaultAsync(s => s.MaMonHoc == request.MaMonHoc.Value);
                    if (subject == null)
                    {
                        _logger.LogWarning("Update task warning: Selected subject {SubjectId} does not exist for User {UserId}.", request.MaMonHoc.Value, userId);
                        request.MaMonHoc = null;
                    }
                }
                catch
                {
                    request.MaMonHoc = null;
                }
            }

            // Auto-resolve or create MonHoc record if TenMonHoc is provided and MaMonHoc is not set
            if (!request.MaMonHoc.HasValue && !string.IsNullOrWhiteSpace(request.TenMonHoc))
            {
                var targetName = request.TenMonHoc.Trim();
                var existingSubject = await _subjectRepository.GetQueryable()
                    .FirstOrDefaultAsync(s => s.TenMonHoc.ToLower() == targetName.ToLower());

                if (existingSubject != null)
                {
                    request.MaMonHoc = existingSubject.MaMonHoc;
                }
                else
                {
                    try
                    {
                        var newSubject = new MonHoc
                        {
                            TenMonHoc = targetName,
                            MaMon = targetName.Length >= 3 ? targetName.Substring(0, 3).ToUpper() : targetName.ToUpper(),
                            MoTa = "Thẻ Môn học tự động tạo",
                            MauSac = !string.IsNullOrWhiteSpace(request.MauSac) ? request.MauSac : "#6366F1",
                            Icon = "book",
                            TrangThai = 1
                        };
                        await _subjectRepository.AddAsync(newSubject);
                        await _subjectRepository.SaveAsync();
                        request.MaMonHoc = newSubject.MaMonHoc;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Could not create MonHoc '{TenMonHoc}': {Message}. Proceeding with MaMonHoc = null.", targetName, ex.Message);
                        request.MaMonHoc = null;
                    }
                }
            }

            task.MaMonHoc = request.MaMonHoc;
            task.TieuDe = request.TieuDe ?? task.TieuDe;
            task.MoTa = request.MoTa ?? string.Empty;
            task.DoUuTien = request.DoUuTien;
            task.TiLeHoanThanh = request.TiLeHoanThanh;
            task.MauSac = request.MauSac ?? string.Empty;
            task.DanhDauQuanTrong = request.DanhDauQuanTrong;
            task.DanhDauYeuThich = request.DanhDauYeuThich;
            task.GhiChu = request.GhiChu ?? string.Empty;
            task.NgayCapNhat = DateTime.UtcNow;

            // Handle status changes
            if (task.TrangThai != request.TrangThai)
            {
                task.TrangThai = request.TrangThai;
                if (request.TrangThai == 3)
                {
                    task.NgayHoanThanh = DateTime.UtcNow;
                }
                else
                {
                    task.NgayHoanThanh = null;
                }
            }

            _taskRepository.Update(task);
            await _taskRepository.SaveAsync();

            _logger.LogInformation("Task {TaskId} updated successfully by User {UserId}.", id, userId);

            return await GetTaskByIdAsync(task.MaCongViec, userId);
        }

        public async Task<TaskDto> UpdateTaskStatusAsync(int id, int userId, byte status)
        {
            var task = await _taskRepository.GetQueryable()
                .FirstOrDefaultAsync(t => t.MaCongViec == id && t.MaNguoiDung == userId);

            if (task == null)
            {
                _logger.LogWarning("Update task status failed: Task {TaskId} not found for User {UserId}.", id, userId);
                throw new NotFoundException("Công việc không tồn tại.");
            }

            if (status > 4)
            {
                throw new BadRequestException("Trạng thái công việc không hợp lệ.");
            }

            var oldStatus = task.TrangThai;
            task.TrangThai = status;
            task.NgayCapNhat = DateTime.UtcNow;

            if (status == 3)
            {
                task.NgayHoanThanh = DateTime.UtcNow;
                task.TiLeHoanThanh = 100;
            }
            else
            {
                task.NgayHoanThanh = null;
                if (status == 0) task.TiLeHoanThanh = 0;
            }

            _taskRepository.Update(task);
            await _taskRepository.SaveAsync();

            // Trigger Realtime Congratulation Notification
            if (status == 3 && oldStatus != 3)
            {
                try
                {
                    await _notificationService.CreateNotificationAsync(new StudyHub.Application.DTOs.Notification.CreateNotificationRequest
                    {
                        MaNguoiDung = userId,
                        MaLoaiThongBao = 1, // Công việc
                        TieuDe = "Chúc mừng hoàn thành công việc",
                        NoiDung = $"Tuyệt vời! Bạn đã hoàn thành 100% công việc: \"{task.TieuDe}\"",
                        DuongDan = "/tasks",
                        MucDo = 1
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể phát thông báo hoàn thành task cho người dùng {UserId}", userId);
                }
            }

            _logger.LogInformation("Task {TaskId} status changed from {OldStatus} to {NewStatus} by User {UserId}.", id, oldStatus, status, userId);

            return await GetTaskByIdAsync(task.MaCongViec, userId);
        }

        public async Task DeleteTaskAsync(int id, int userId)
        {
            var task = await _taskRepository.GetQueryable()
                .FirstOrDefaultAsync(t => t.MaCongViec == id && t.MaNguoiDung == userId);

            if (task == null)
            {
                _logger.LogWarning("Delete task failed: Task {TaskId} not found for User {UserId}.", id, userId);
                throw new NotFoundException("Công việc không tồn tại.");
            }

            _taskRepository.Delete(task);
            await _taskRepository.SaveAsync();

            _logger.LogInformation("Task {TaskId} deleted successfully by User {UserId}.", id, userId);
        }
    }
}
