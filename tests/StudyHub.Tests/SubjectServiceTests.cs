using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.DTOs.Subject;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class SubjectServiceTests
    {
        private readonly Mock<IGenericRepository<MonHoc>> _mockSubjectRepo;
        private readonly Mock<IGenericRepository<CongViec>> _mockTaskRepo;
        private readonly SubjectService _subjectService;

        public SubjectServiceTests()
        {
            _mockSubjectRepo = new Mock<IGenericRepository<MonHoc>>();
            _mockTaskRepo = new Mock<IGenericRepository<CongViec>>();
            _subjectService = new SubjectService(_mockSubjectRepo.Object, _mockTaskRepo.Object);
        }

        [Fact]
        public async Task GetSubjectsAsync_ShouldReturnSubjects_WithCorrectProgressCalculations()
        {
            // Arrange
            var userId = 1;
            var subjects = new List<MonHoc>
            {
                new MonHoc { MaMonHoc = 1, TenMonHoc = "Discrete Math", MaMon = "MAD", TrangThai = 1 },
                new MonHoc { MaMonHoc = 2, TenMonHoc = "Computer Architecture", MaMon = "CEA", TrangThai = 1 }
            };

            var tasks = new List<CongViec>
            {
                new CongViec { MaCongViec = 1, MaNguoiDung = userId, MaMonHoc = 1, TrangThai = 3 }, // Completed
                new CongViec { MaCongViec = 2, MaNguoiDung = userId, MaMonHoc = 1, TrangThai = 1 }, // In Progress
                new CongViec { MaCongViec = 3, MaNguoiDung = userId, MaMonHoc = 2, TrangThai = 1 }  // In Progress
            };

            _mockSubjectRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MonHoc, bool>>>()))
                .ReturnsAsync(subjects);
            _mockTaskRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CongViec, bool>>>()))
                .ReturnsAsync(tasks);

            // Act
            var result = await _subjectService.GetSubjectsAsync(userId);

            // Assert
            Assert.Equal(2, result.Count);
            
            var subject1 = result.First(s => s.MaMonHoc == 1);
            Assert.Equal(2, subject1.TaskCount);
            Assert.Equal(50, subject1.Progress); // 1 completed out of 2 total = 50%

            var subject2 = result.First(s => s.MaMonHoc == 2);
            Assert.Equal(1, subject2.TaskCount);
            Assert.Equal(0, subject2.Progress); // 0 completed out of 1 total = 0%
        }

        [Fact]
        public async Task CreateSubjectAsync_ShouldThrowBadRequestException_WhenSubjectCodeExists()
        {
            // Arrange
            var request = new CreateSubjectRequest
            {
                TenMonHoc = "Discrete Math",
                MaMon = "MAD",
                MauSac = "#6366F1",
                Icon = "pi-book"
            };

            _mockSubjectRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MonHoc, bool>>>()))
                .ReturnsAsync(new List<MonHoc> { new MonHoc { MaMonHoc = 1, MaMon = "MAD" } });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _subjectService.CreateSubjectAsync(request));
        }

        [Fact]
        public async Task DeleteSubjectAsync_ShouldThrowBadRequestException_WhenTasksAreLinked()
        {
            // Arrange
            var subjectId = 1;
            var subject = new MonHoc { MaMonHoc = subjectId, TenMonHoc = "Discrete Math" };

            _mockSubjectRepo.Setup(r => r.GetByIdAsync(subjectId))
                .ReturnsAsync(subject);
            _mockTaskRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CongViec, bool>>>()))
                .ReturnsAsync(new List<CongViec> { new CongViec { MaCongViec = 1, MaMonHoc = subjectId } });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _subjectService.DeleteSubjectAsync(subjectId));
            Assert.Equal("Không thể xóa môn học vì đang có công việc liên kết.", ex.Message);
        }

        [Fact]
        public async Task DeleteSubjectAsync_ShouldSuccessfullyDelete_WhenNoTasksAreLinked()
        {
            // Arrange
            var subjectId = 1;
            var subject = new MonHoc { MaMonHoc = subjectId, TenMonHoc = "Discrete Math" };

            _mockSubjectRepo.Setup(r => r.GetByIdAsync(subjectId))
                .ReturnsAsync(subject);
            _mockTaskRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CongViec, bool>>>()))
                .ReturnsAsync(new List<CongViec>()); // Empty tasks list

            // Act
            await _subjectService.DeleteSubjectAsync(subjectId);

            // Assert
            _mockSubjectRepo.Verify(r => r.Delete(subject), Times.Once);
            _mockSubjectRepo.Verify(r => r.SaveAsync(), Times.Once);
        }
    }
}
