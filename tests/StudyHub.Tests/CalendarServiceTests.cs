using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.DTOs.Calendar;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class CalendarServiceTests
    {
        private readonly Mock<ISuKienRepository> _mockEventRepo;
        private readonly Mock<ILichHocRepository> _mockClassRepo;
        private readonly Mock<ILichThiRepository> _mockExamRepo;
        private readonly Mock<IGenericRepository<CongViec>> _mockTaskRepo;
        private readonly Mock<ILogger<CalendarService>> _mockLogger;
        private readonly CalendarService _service;

        public CalendarServiceTests()
        {
            _mockEventRepo = new Mock<ISuKienRepository>();
            _mockClassRepo = new Mock<ILichHocRepository>();
            _mockExamRepo = new Mock<ILichThiRepository>();
            _mockTaskRepo = new Mock<IGenericRepository<CongViec>>();
            _mockLogger = new Mock<ILogger<CalendarService>>();

            _service = new CalendarService(
                _mockEventRepo.Object,
                _mockClassRepo.Object,
                _mockExamRepo.Object,
                _mockTaskRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetCalendarEventsAsync_ShouldAggregateEventsFromAllSources()
        {
            // Arrange
            var userId = 1;
            var start = DateTime.Now.AddDays(-5);
            var end = DateTime.Now.AddDays(5);

            _mockEventRepo.Setup(r => r.GetQueryable()).Returns(new List<SuKien>
            {
                new SuKien { MaSuKien = 1, MaNguoiDung = userId, TieuDe = "Họp nhóm", ThoiGianBatDau = DateTime.Now, ThoiGianKetThuc = DateTime.Now.AddHours(1), TrangThai = 1 }
            }.AsQueryable());

            _mockClassRepo.Setup(r => r.GetQueryable()).Returns(new List<LichHoc>
            {
                new LichHoc { MaLichHoc = 1, MaNguoiDung = userId, NgayBatDau = DateTime.Now.AddDays(-1), NgayKetThuc = DateTime.Now.AddDays(1) }
            }.AsQueryable());

            _mockExamRepo.Setup(r => r.GetQueryable()).Returns(new List<LichThi>
            {
                new LichThi { MaLichThi = 1, MaNguoiDung = userId, HinhThucThi = "Cuối kỳ", NgayThi = DateTime.Now.AddDays(2) }
            }.AsQueryable());

            _mockTaskRepo.Setup(r => r.GetQueryable()).Returns(new List<CongViec>
            {
                new CongViec { MaCongViec = 1, MaNguoiDung = userId, TieuDe = "Nộp bài tập", HanHoanThanh = DateTime.Now.AddDays(3), DaXoa = false }
            }.AsQueryable());

            // Act
            var events = await _service.GetCalendarEventsAsync(userId, start, end);

            // Assert
            Assert.NotNull(events);
            Assert.Equal(4, events.Count());
        }

        [Fact]
        public async Task CreateEventAsync_ShouldAddPersonalEvent()
        {
            // Arrange
            var userId = 1;
            _mockEventRepo.Setup(r => r.AddAsync(It.IsAny<SuKien>())).Returns(Task.CompletedTask);

            var request = new CreateCalendarEventRequest
            {
                TieuDe = "Ôn thi Giải tích",
                ThoiGianBatDau = DateTime.Now,
                ThoiGianKetThuc = DateTime.Now.AddHours(2)
            };

            // Act
            var result = await _service.CreateEventAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ôn thi Giải tích", result.Title);
            Assert.True(result.IsEditable);
            _mockEventRepo.Verify(r => r.AddAsync(It.IsAny<SuKien>()), Times.Once);
        }

        [Fact]
        public async Task DeleteEventAsync_ShouldRemovePersonalEvent_WhenExists()
        {
            // Arrange
            var userId = 1;
            var eventId = 10;
            var suKien = new SuKien { MaSuKien = eventId, MaNguoiDung = userId, TieuDe = "Event" };

            _mockEventRepo.Setup(r => r.GetQueryable()).Returns(new List<SuKien> { suKien }.AsQueryable());

            // Act
            await _service.DeleteEventAsync(eventId, userId);

            // Assert
            _mockEventRepo.Verify(r => r.Delete(suKien), Times.Once);
            _mockEventRepo.Verify(r => r.SaveAsync(), Times.Once);
        }
    }
}
