using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services.Ai
{
    public class AiScheduleContextProvider : IAiScheduleContextProvider
    {
        private readonly ILichHocRepository _classRepository;
        private readonly ILichThiRepository _examRepository;
        private readonly ISuKienRepository _eventRepository;
        private readonly IGenericRepository<CongViec> _taskRepository;

        public AiScheduleContextProvider(
            ILichHocRepository classRepository,
            ILichThiRepository examRepository,
            ISuKienRepository eventRepository,
            IGenericRepository<CongViec> taskRepository)
        {
            _classRepository = classRepository;
            _examRepository = examRepository;
            _eventRepository = eventRepository;
            _taskRepository = taskRepository;
        }

        public async Task<AiScheduleContextResult> GetScheduleContextAsync(int userId, AiDateResolution dateResolution)
        {
            var result = new AiScheduleContextResult
            {
                TargetStartDate = dateResolution.TargetStartDate,
                TargetEndDate = dateResolution.TargetEndDate,
                TargetDateLabel = dateResolution.Label,
                Scope = dateResolution.Scope
            };

            var startDay = dateResolution.TargetStartDate.Date;
            var endDay = dateResolution.TargetEndDate.Date;

            // Selective Retrieval Flags based strictly on QueryType
            bool fetchClasses = dateResolution.QueryType == "ClassSchedule" || dateResolution.QueryType == "All";
            bool fetchExams = dateResolution.QueryType == "ExamSchedule" || dateResolution.QueryType == "All";
            bool fetchEvents = dateResolution.QueryType == "Event" || dateResolution.QueryType == "All";
            bool fetchDeadlines = dateResolution.QueryType == "All";

            if (dateResolution.Scope == "SingleDay")
            {
                var targetThu = dateResolution.TargetThu;

                // 1. Query Classes (LichHoc) if requested
                if (fetchClasses)
                {
                    result.Classes = await _classRepository.GetQueryable()
                        .AsNoTracking()
                        .Include(c => c.MonHoc)
                        .Where(c => c.MaNguoiDung == userId && !c.DaXoa &&
                                   c.Thu == targetThu &&
                                   c.NgayBatDau.Date <= startDay && c.NgayKetThuc.Date >= startDay)
                        .OrderBy(c => c.TietBatDau)
                        .ThenBy(c => c.NgayBatDau)
                        .Select(c => new AiClassScheduleDto
                        {
                            MaLichHoc = c.MaLichHoc,
                            TenMonHoc = !string.IsNullOrWhiteSpace(c.TieuDe) ? c.TieuDe : (c.MonHoc != null ? c.MonHoc.TenMonHoc : "Môn học"),
                            PhongHoc = c.PhongHoc ?? string.Empty,
                            GiangVien = c.GiangVien ?? string.Empty,
                            Thu = c.Thu,
                            TietBatDau = c.TietBatDau,
                            TietKetThuc = c.TietKetThuc,
                            NgayBatDau = c.NgayBatDau,
                            NgayKetThuc = c.NgayKetThuc,
                            MauSac = c.MauSac
                        })
                        .ToListAsync();
                }

                // 2. Query Exams (LichThi) if requested (PRIORITIZE TieuDe over MonHoc.TenMonHoc)
                if (fetchExams)
                {
                    result.Exams = await _examRepository.GetQueryable()
                        .AsNoTracking()
                        .Include(e => e.MonHoc)
                        .Where(e => e.MaNguoiDung == userId && !e.DaXoa &&
                                   e.NgayThi.Date == startDay)
                        .OrderBy(e => e.NgayThi)
                        .Select(e => new AiExamScheduleDto
                        {
                            MaLichThi = e.MaLichThi,
                            TenMonHoc = !string.IsNullOrWhiteSpace(e.TieuDe) ? e.TieuDe : (e.MonHoc != null ? e.MonHoc.TenMonHoc : "Môn thi"),
                            HinhThucThi = e.HinhThucThi ?? string.Empty,
                            NgayThi = e.NgayThi,
                            ThoiLuong = e.ThoiLuong,
                            PhongThi = e.PhongThi ?? string.Empty,
                            GiangVien = e.GiangVien
                        })
                        .ToListAsync();
                }

                // 3. Query Personal Events (SuKien) if requested
                if (fetchEvents)
                {
                    result.Events = await _eventRepository.GetQueryable()
                        .AsNoTracking()
                        .Where(s => s.MaNguoiDung == userId && !s.DaXoa && s.TrangThai == 1 &&
                                   s.ThoiGianBatDau.Date <= startDay && s.ThoiGianKetThuc.Date >= startDay)
                        .OrderBy(s => s.ThoiGianBatDau)
                        .Select(s => new AiPersonalEventDto
                        {
                            MaSuKien = s.MaSuKien,
                            TieuDe = s.TieuDe,
                            MoTa = s.MoTa ?? string.Empty,
                            ThoiGianBatDau = s.ThoiGianBatDau,
                            ThoiGianKetThuc = s.ThoiGianKetThuc,
                            DiaDiem = s.DiaDiem ?? string.Empty
                        })
                        .ToListAsync();
                }

                // 4. Query Deadlines (CongViec) if requested
                if (fetchDeadlines)
                {
                    result.Deadlines = await _taskRepository.GetQueryable()
                        .AsNoTracking()
                        .Include(t => t.MonHoc)
                        .Where(t => t.MaNguoiDung == userId && !t.DaXoa && t.TrangThai != 3 &&
                                   t.HanHoanThanh.HasValue && t.HanHoanThanh.Value.Date == startDay)
                        .OrderBy(t => t.DoUuTien)
                        .Select(t => new AiTaskScheduleDto
                        {
                            MaCongViec = t.MaCongViec,
                            TieuDe = t.TieuDe,
                            TenMonHoc = t.MonHoc != null ? t.MonHoc.TenMonHoc : null,
                            HanHoanThanh = t.HanHoanThanh,
                            DoUuTien = t.DoUuTien
                        })
                        .ToListAsync();
                }
            }
            else
            {
                // Week Scope: Selective retrieval across the week range [startDay, endDay]
                if (fetchClasses)
                {
                    result.Classes = await _classRepository.GetQueryable()
                        .AsNoTracking()
                        .Include(c => c.MonHoc)
                        .Where(c => c.MaNguoiDung == userId && !c.DaXoa &&
                                   c.NgayBatDau.Date <= endDay && c.NgayKetThuc.Date >= startDay)
                        .OrderBy(c => c.Thu)
                        .ThenBy(c => c.TietBatDau)
                        .Select(c => new AiClassScheduleDto
                        {
                            MaLichHoc = c.MaLichHoc,
                            TenMonHoc = !string.IsNullOrWhiteSpace(c.TieuDe) ? c.TieuDe : (c.MonHoc != null ? c.MonHoc.TenMonHoc : "Môn học"),
                            PhongHoc = c.PhongHoc ?? string.Empty,
                            GiangVien = c.GiangVien ?? string.Empty,
                            Thu = c.Thu,
                            TietBatDau = c.TietBatDau,
                            TietKetThuc = c.TietKetThuc,
                            NgayBatDau = c.NgayBatDau,
                            NgayKetThuc = c.NgayKetThuc,
                            MauSac = c.MauSac
                        })
                        .ToListAsync();
                }

                if (fetchExams)
                {
                    result.Exams = await _examRepository.GetQueryable()
                        .AsNoTracking()
                        .Include(e => e.MonHoc)
                        .Where(e => e.MaNguoiDung == userId && !e.DaXoa &&
                                   e.NgayThi.Date >= startDay && e.NgayThi.Date <= endDay)
                        .OrderBy(e => e.NgayThi)
                        .Select(e => new AiExamScheduleDto
                        {
                            MaLichThi = e.MaLichThi,
                            TenMonHoc = !string.IsNullOrWhiteSpace(e.TieuDe) ? e.TieuDe : (e.MonHoc != null ? e.MonHoc.TenMonHoc : "Môn thi"),
                            HinhThucThi = e.HinhThucThi ?? string.Empty,
                            NgayThi = e.NgayThi,
                            ThoiLuong = e.ThoiLuong,
                            PhongThi = e.PhongThi ?? string.Empty,
                            GiangVien = e.GiangVien
                        })
                        .ToListAsync();
                }
            }

            return result;
        }
    }
}
