using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Subject;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly IGenericRepository<MonHoc> _subjectRepository;
        private readonly IGenericRepository<CongViec> _taskRepository;

        public SubjectService(
            IGenericRepository<MonHoc> subjectRepository,
            IGenericRepository<CongViec> taskRepository)
        {
            _subjectRepository = subjectRepository;
            _taskRepository = taskRepository;
        }

        public async Task<List<SubjectDto>> GetSubjectsAsync(int userId)
        {
            var subjects = await _subjectRepository.FindAsync(s => s.TrangThai == 1);
            var userTasks = await _taskRepository.FindAsync(t => t.MaNguoiDung == userId);

            return subjects.Select(s => {
                var relatedTasks = userTasks.Where(t => t.MaMonHoc == s.MaMonHoc).ToList();
                var taskCount = relatedTasks.Count;
                var completedCount = relatedTasks.Count(t => t.TrangThai == 3);
                var progress = taskCount > 0 ? (completedCount * 100) / taskCount : 0;

                return new SubjectDto
                {
                    MaMonHoc = s.MaMonHoc,
                    TenMonHoc = s.TenMonHoc,
                    MaMon = s.MaMon,
                    MoTa = s.MoTa,
                    MauSac = s.MauSac,
                    Icon = s.Icon,
                    TrangThai = s.TrangThai,
                    TaskCount = taskCount,
                    Progress = progress
                };
            }).ToList();
        }

        public async Task<SubjectDto> GetSubjectByIdAsync(int id, int userId)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                throw new NotFoundException("Môn học không tồn tại.");
            }

            var userTasks = await _taskRepository.FindAsync(t => t.MaNguoiDung == userId && t.MaMonHoc == id);
            var taskCount = userTasks.Count();
            var completedCount = userTasks.Count(t => t.TrangThai == 3);
            var progress = taskCount > 0 ? (completedCount * 100) / taskCount : 0;

            return new SubjectDto
            {
                MaMonHoc = subject.MaMonHoc,
                TenMonHoc = subject.TenMonHoc,
                MaMon = subject.MaMon,
                MoTa = subject.MoTa,
                MauSac = subject.MauSac,
                Icon = subject.Icon,
                TrangThai = subject.TrangThai,
                TaskCount = taskCount,
                Progress = progress
            };
        }

        public async Task<SubjectDto> CreateSubjectAsync(CreateSubjectRequest request)
        {
            var existing = await _subjectRepository.FindAsync(s => s.MaMon == request.MaMon);
            if (existing.Any())
            {
                throw new BadRequestException("Mã môn học đã tồn tại.");
            }

            var subject = new MonHoc
            {
                TenMonHoc = request.TenMonHoc,
                MaMon = request.MaMon,
                MoTa = request.MoTa,
                MauSac = request.MauSac,
                Icon = request.Icon,
                TrangThai = 1, // Active by default
                NgayTao = DateTime.UtcNow
            };

            await _subjectRepository.AddAsync(subject);
            await _subjectRepository.SaveAsync();

            return new SubjectDto
            {
                MaMonHoc = subject.MaMonHoc,
                TenMonHoc = subject.TenMonHoc,
                MaMon = subject.MaMon,
                MoTa = subject.MoTa,
                MauSac = subject.MauSac,
                Icon = subject.Icon,
                TrangThai = subject.TrangThai,
                TaskCount = 0,
                Progress = 0
            };
        }

        public async Task<SubjectDto> UpdateSubjectAsync(int id, UpdateSubjectRequest request)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                throw new NotFoundException("Môn học không tồn tại.");
            }

            var existing = await _subjectRepository.FindAsync(s => s.MaMon == request.MaMon && s.MaMonHoc != id);
            if (existing.Any())
            {
                throw new BadRequestException("Mã môn học đã tồn tại.");
            }

            subject.TenMonHoc = request.TenMonHoc;
            subject.MaMon = request.MaMon;
            subject.MoTa = request.MoTa;
            subject.MauSac = request.MauSac;
            subject.Icon = request.Icon;
            subject.TrangThai = request.TrangThai;
            subject.NgayCapNhat = DateTime.UtcNow;

            _subjectRepository.Update(subject);
            await _subjectRepository.SaveAsync();

            return new SubjectDto
            {
                MaMonHoc = subject.MaMonHoc,
                TenMonHoc = subject.TenMonHoc,
                MaMon = subject.MaMon,
                MoTa = subject.MoTa,
                MauSac = subject.MauSac,
                Icon = subject.Icon,
                TrangThai = subject.TrangThai,
                TaskCount = 0, // Will be updated on subsequent fetches
                Progress = 0
            };
        }

        public async Task DeleteSubjectAsync(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                throw new NotFoundException("Môn học không tồn tại.");
            }

            var linkedTasks = await _taskRepository.FindAsync(t => t.MaMonHoc == id);
            if (linkedTasks.Any())
            {
                throw new BadRequestException("Không thể xóa môn học vì đang có công việc liên kết.");
            }

            _subjectRepository.Delete(subject);
            await _subjectRepository.SaveAsync();
        }
    }
}
