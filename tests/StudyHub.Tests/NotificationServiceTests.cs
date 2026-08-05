using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.DTOs.Notification;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IThongBaoRepository> _mockNotificationRepo;
        private readonly Mock<ILoaiThongBaoRepository> _mockTypeRepo;
        private readonly Mock<IGenericRepository<NguoiDung>> _mockUserRepo;
        private readonly Mock<ILogger<NotificationService>> _mockLogger;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _mockNotificationRepo = new Mock<IThongBaoRepository>();
            _mockTypeRepo = new Mock<ILoaiThongBaoRepository>();
            _mockUserRepo = new Mock<IGenericRepository<NguoiDung>>();
            _mockLogger = new Mock<ILogger<NotificationService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            _service = new NotificationService(
                _mockNotificationRepo.Object,
                _mockTypeRepo.Object,
                _mockUserRepo.Object,
                _mockLogger.Object,
                _mockServiceProvider.Object
            );
        }

        [Fact]
        public async Task GetUnreadCountAsync_ShouldReturnCorrectUnreadCount()
        {
            // Arrange
            var userId = 1;
            _mockNotificationRepo.Setup(r => r.GetQueryable()).Returns(new List<ThongBao>
            {
                new ThongBao { MaThongBao = 1, MaNguoiDung = userId, DaDoc = false, DaXoa = false },
                new ThongBao { MaThongBao = 2, MaNguoiDung = userId, DaDoc = true, DaXoa = false },
                new ThongBao { MaThongBao = 3, MaNguoiDung = userId, DaDoc = false, DaXoa = false },
                new ThongBao { MaThongBao = 4, MaNguoiDung = 2, DaDoc = false, DaXoa = false }
            }.AsQueryable());

            // Act
            var count = await _service.GetUnreadCountAsync(userId);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldUpdateDaDocAndNgayDoc()
        {
            // Arrange
            var userId = 1;
            var notificationId = 10;
            var notification = new ThongBao
            {
                MaThongBao = notificationId,
                MaNguoiDung = userId,
                TieuDe = "Test",
                NoiDung = "Content",
                DaDoc = false,
                DaXoa = false
            };

            _mockNotificationRepo.Setup(r => r.GetQueryable()).Returns(new List<ThongBao> { notification }.AsQueryable());

            // Act
            var result = await _service.MarkAsReadAsync(notificationId, userId);

            // Assert
            Assert.True(result.DaDoc);
            Assert.NotNull(result.NgayDoc);
            _mockNotificationRepo.Verify(r => r.Update(It.Is<ThongBao>(n => n.MaThongBao == notificationId && n.DaDoc)), Times.Once);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_ShouldMarkAllUnreadNotificationsAsRead()
        {
            // Arrange
            var userId = 1;
            var list = new List<ThongBao>
            {
                new ThongBao { MaThongBao = 1, MaNguoiDung = userId, DaDoc = false, DaXoa = false },
                new ThongBao { MaThongBao = 2, MaNguoiDung = userId, DaDoc = false, DaXoa = false }
            };

            _mockNotificationRepo.Setup(r => r.GetQueryable()).Returns(list.AsQueryable());

            // Act
            await _service.MarkAllAsReadAsync(userId);

            // Assert
            Assert.All(list, item => Assert.True(item.DaDoc));
            _mockNotificationRepo.Verify(r => r.SaveAsync(), Times.Once);
        }
    }
}
