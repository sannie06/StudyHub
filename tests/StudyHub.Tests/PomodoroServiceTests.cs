using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.DTOs.Pomodoro;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class PomodoroServiceTests
    {
        private readonly Mock<IGenericRepository<PomodoroSession>> _mockPomodoroRepo;
        private readonly Mock<IGenericRepository<CongViec>> _mockTaskRepo;
        private readonly Mock<IGenericRepository<CauHinhHeThong>> _mockConfigRepo;
        private readonly Mock<IGenericRepository<ThongKeHocTap>> _mockStatsRepo;
        private readonly Mock<ILogger<PomodoroService>> _mockLogger;
        private readonly PomodoroService _pomodoroService;

        public PomodoroServiceTests()
        {
            _mockPomodoroRepo = new Mock<IGenericRepository<PomodoroSession>>();
            _mockTaskRepo = new Mock<IGenericRepository<CongViec>>();
            _mockConfigRepo = new Mock<IGenericRepository<CauHinhHeThong>>();
            _mockStatsRepo = new Mock<IGenericRepository<ThongKeHocTap>>();
            _mockLogger = new Mock<ILogger<PomodoroService>>();

            _pomodoroService = new PomodoroService(
                _mockPomodoroRepo.Object,
                _mockTaskRepo.Object,
                _mockConfigRepo.Object,
                _mockStatsRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task StartSessionAsync_ShouldCancelActiveSession_AndCreateNewOne()
        {
            // Arrange
            var userId = 1;
            var activeSession = new PomodoroSession { MaSession = 5, MaNguoiDung = userId, TrangThai = 2 };
            _mockPomodoroRepo.Setup(r => r.GetQueryable())
                .Returns(new List<PomodoroSession> { activeSession }.AsQueryable());

            var request = new StartPomodoroRequest
            {
                MaCongViec = 12,
                LoaiSession = 0,
                ThoiLuong = 25,
                TieuDe = "Focus"
            };

            // Act
            var result = await _pomodoroService.StartSessionAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, activeSession.TrangThai); // Cancelled
            _mockPomodoroRepo.Verify(r => r.AddAsync(It.IsAny<PomodoroSession>()), Times.Once);
            _mockPomodoroRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task FinishSessionAsync_ShouldAutoUpdateTaskProgress_AndPrecalculateStats()
        {
            // Arrange
            var userId = 1;
            var sessionId = 10;
            var taskId = 99;
            var session = new PomodoroSession
            {
                MaSession = sessionId,
                MaNguoiDung = userId,
                MaCongViec = taskId,
                LoaiSession = 0, // Focus
                ThoiLuong = 25,
                TrangThai = 2 // Running
            };

            _mockPomodoroRepo.Setup(r => r.GetQueryable())
                .Returns(new List<PomodoroSession> { session }.AsQueryable());

            var task = new CongViec
            {
                MaCongViec = taskId,
                MaNguoiDung = userId,
                TiLeHoanThanh = 50,
                TrangThai = 1
            };
            _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            // Setup default Pomodoro progress configuration of 20%
            var configList = new List<CauHinhHeThong>
            {
                new CauHinhHeThong { TenCauHinh = "PomodoroProgressIncrement", GiaTri = "20" }
            };
            _mockConfigRepo.Setup(r => r.GetQueryable())
                .Returns(configList.AsQueryable());

            // Setup empty stats
            _mockStatsRepo.Setup(r => r.GetQueryable())
                .Returns(new List<ThongKeHocTap>().AsQueryable());

            var request = new FinishPomodoroRequest { TongThoiGianTamDung = 0, SoLanTamDung = 0 };

            // Act
            var result = await _pomodoroService.FinishSessionAsync(sessionId, userId, request);

            // Assert
            Assert.Equal(1, session.TrangThai); // Completed
            Assert.Equal(70, task.TiLeHoanThanh); // 50% + 20% increment = 70%
            _mockTaskRepo.Verify(r => r.Update(task), Times.Once);
            _mockStatsRepo.Verify(r => r.AddAsync(It.IsAny<ThongKeHocTap>()), Times.Once); // Daily stats precalculated
        }

        [Fact]
        public async Task GetActiveSessionAsync_ShouldReturnActiveSession_WhenExists()
        {
            // Arrange
            var userId = 1;
            var session = new PomodoroSession { MaSession = 20, MaNguoiDung = userId, TrangThai = 2 };
            _mockPomodoroRepo.Setup(r => r.GetQueryable())
                .Returns(new List<PomodoroSession> { session }.AsQueryable());

            // Act
            var result = await _pomodoroService.GetActiveSessionAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.MaSession);
        }
    }
}
