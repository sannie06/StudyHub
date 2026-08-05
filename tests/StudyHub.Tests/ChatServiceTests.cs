using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.DTOs.Chat;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class ChatServiceTests
    {
        private readonly Mock<ITinNhanRepository> _mockChatRepo;
        private readonly Mock<IThanhVienNhomRepository> _mockMemberRepo;
        private readonly Mock<INhomHocTapRepository> _mockGroupRepo;
        private readonly Mock<IGenericRepository<NguoiDung>> _mockUserRepo;
        private readonly Mock<ILogger<ChatService>> _mockLogger;
        private readonly ChatService _service;

        public ChatServiceTests()
        {
            _mockChatRepo = new Mock<ITinNhanRepository>();
            _mockMemberRepo = new Mock<IThanhVienNhomRepository>();
            _mockGroupRepo = new Mock<INhomHocTapRepository>();
            _mockUserRepo = new Mock<IGenericRepository<NguoiDung>>();
            _mockLogger = new Mock<ILogger<ChatService>>();

            _service = new ChatService(
                _mockChatRepo.Object,
                _mockMemberRepo.Object,
                _mockGroupRepo.Object,
                _mockUserRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task SendMessageAsync_ShouldSendMessage_WhenUserIsGroupMember()
        {
            // Arrange
            var userId = 1;
            var groupId = 10;

            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(new List<ThanhVienNhom>
            {
                new ThanhVienNhom { MaNhom = groupId, MaNguoiDung = userId, TrangThai = 1 }
            }.AsQueryable());

            _mockUserRepo.Setup(r => r.GetQueryable()).Returns(new List<NguoiDung>
            {
                new NguoiDung { MaNguoiDung = userId, HoTen = "John Doe" }
            }.AsQueryable());

            _mockChatRepo.Setup(r => r.AddAsync(It.IsAny<TinNhan>())).Returns(Task.CompletedTask);

            var request = new SendChatMessageRequest
            {
                MaNhom = groupId,
                NoiDung = "Hello team!"
            };

            // Act
            var result = await _service.SendMessageAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello team!", result.NoiDung);
            Assert.Equal("John Doe", result.TenNguoiGui);
            Assert.True(result.IsMine);

            _mockChatRepo.Verify(r => r.AddAsync(It.IsAny<TinNhan>()), Times.Once);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldThrowUnauthorizedException_WhenUserIsNotGroupMember()
        {
            // Arrange
            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(new List<ThanhVienNhom>().AsQueryable());
            var request = new SendChatMessageRequest { MaNhom = 10, NoiDung = "Hello" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _service.SendMessageAsync(userId: 1, request));
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldMarkAsDeleted_WhenUserIsSender()
        {
            // Arrange
            var userId = 1;
            var messageId = 100;
            var message = new TinNhan
            {
                MaTinNhan = messageId,
                MaNhom = 10,
                MaNguoiGui = userId,
                NoiDung = "Old message",
                DaXoa = false
            };

            _mockChatRepo.Setup(r => r.GetQueryable()).Returns(new List<TinNhan> { message }.AsQueryable());

            // Act
            await _service.DeleteMessageAsync(messageId, userId);

            // Assert
            Assert.True(message.DaXoa);
            _mockChatRepo.Verify(r => r.Update(It.Is<TinNhan>(m => m.MaTinNhan == messageId && m.DaXoa)), Times.Once);
        }
    }
}
