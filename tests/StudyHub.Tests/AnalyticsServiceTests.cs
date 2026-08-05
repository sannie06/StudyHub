using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Subject;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class AnalyticsServiceTests
    {
        private readonly Mock<IGenericRepository<ThongKeHocTap>> _mockStatsRepo;
        private readonly Mock<IGenericRepository<CongViec>> _mockTaskRepo;
        private readonly Mock<ISubjectService> _mockSubjectService;
        private readonly AnalyticsService _analyticsService;

        public AnalyticsServiceTests()
        {
            _mockStatsRepo = new Mock<IGenericRepository<ThongKeHocTap>>();
            _mockTaskRepo = new Mock<IGenericRepository<CongViec>>();
            _mockSubjectService = new Mock<ISubjectService>();

            _analyticsService = new AnalyticsService(
                _mockStatsRepo.Object,
                _mockTaskRepo.Object,
                _mockSubjectService.Object
            );
        }

        [Fact]
        public async Task GetUserAnalyticsAsync_ShouldPrecalculateMetricsCorrectly()
        {
            // Arrange
            var userId = 1;
            var today = DateTime.UtcNow.Date;

            var statsRecords = new List<ThongKeHocTap>
            {
                new ThongKeHocTap
                {
                    MaNguoiDung = userId,
                    TongPhutHoc = 50,
                    TongPomodoro = 2,
                    DiemNangSuat = 70,
                    SoNgayHocLienTiep = 3,
                    NgayThongKe = today
                },
                new ThongKeHocTap
                {
                    MaNguoiDung = userId,
                    TongPhutHoc = 25,
                    TongPomodoro = 1,
                    DiemNangSuat = 35,
                    SoNgayHocLienTiep = 2,
                    NgayThongKe = today.AddDays(-1)
                }
            };

            _mockStatsRepo.Setup(r => r.GetQueryable())
                .Returns(statsRecords.AsQueryable());

            _mockTaskRepo.Setup(r => r.GetQueryable())
                .Returns(new List<CongViec>
                {
                    new CongViec { MaNguoiDung = userId, TrangThai = 3 },
                    new CongViec { MaNguoiDung = userId, TrangThai = 1 }
                }.AsQueryable());

            _mockSubjectService.Setup(s => s.GetSubjectsAsync(userId))
                .ReturnsAsync(new List<SubjectDto>
                {
                    new SubjectDto { MaMonHoc = 10, TenMonHoc = "Subject A", Progress = 50 }
                });

            // Act
            var result = await _analyticsService.GetUserAnalyticsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(75, result.TotalFocusMinutes); // 50 + 25
            Assert.Equal(3, result.TotalPomodoros); // 2 + 1
            Assert.Equal(3, result.CurrentStreak);
            Assert.Equal(105, result.ProductivityScore); // 70 + 35
            Assert.Equal(50, result.TaskCompletionRate); // 1 completed out of 2 = 50%
            Assert.Single(result.SubjectProgress);
            Assert.Equal(7, result.WeeklyActivity.Count);
            Assert.Equal(365, result.HeatMap.Count);
        }
    }
}
