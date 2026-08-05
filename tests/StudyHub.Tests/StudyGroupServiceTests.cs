using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.DTOs.StudyGroup;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class StudyGroupServiceTests
    {
        private readonly Mock<INhomHocTapRepository> _mockGroupRepo;
        private readonly Mock<IThanhVienNhomRepository> _mockMemberRepo;
        private readonly Mock<IGenericRepository<NguoiDung>> _mockUserRepo;
        private readonly Mock<IGenericRepository<MonHoc>> _mockSubjectRepo;
        private readonly Mock<ILogger<StudyGroupService>> _mockLogger;
        private readonly StudyGroupService _service;

        public StudyGroupServiceTests()
        {
            _mockGroupRepo = new Mock<INhomHocTapRepository>();
            _mockMemberRepo = new Mock<IThanhVienNhomRepository>();
            _mockUserRepo = new Mock<IGenericRepository<NguoiDung>>();
            _mockSubjectRepo = new Mock<IGenericRepository<MonHoc>>();
            _mockLogger = new Mock<ILogger<StudyGroupService>>();

            _service = new StudyGroupService(
                _mockGroupRepo.Object,
                _mockMemberRepo.Object,
                _mockUserRepo.Object,
                _mockSubjectRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateGroupAsync_ShouldCreateGroup_AndSetUserAsOwner()
        {
            // Arrange
            var userId = 1;
            var request = new CreateStudyGroupRequest
            {
                TenNhom = "Algorithms Study Group",
                MoTa = "Preparing for DSA exam",
                SoLuongToiDa = 15
            };

            _mockGroupRepo.Setup(r => r.GetQueryable()).Returns(new List<NhomHocTap>().AsQueryable());
            _mockUserRepo.Setup(r => r.GetQueryable()).Returns(new List<NguoiDung>
            {
                new NguoiDung { MaNguoiDung = userId, HoTen = "Alice Smith" }
            }.AsQueryable());

            _mockGroupRepo.Setup(r => r.AddAsync(It.IsAny<NhomHocTap>())).Returns(Task.CompletedTask);
            _mockMemberRepo.Setup(r => r.AddAsync(It.IsAny<ThanhVienNhom>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateGroupAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Algorithms Study Group", result.TenNhom);
            Assert.Equal("Alice Smith", result.TenNguoiTao);
            Assert.True(result.IsOwner);
            Assert.Equal(6, result.MaThamGia.Length);

            _mockGroupRepo.Verify(r => r.AddAsync(It.IsAny<NhomHocTap>()), Times.Once);
            _mockMemberRepo.Verify(r => r.AddAsync(It.IsAny<ThanhVienNhom>()), Times.Once);
        }

        [Fact]
        public async Task JoinGroupViaCodeAsync_ShouldThrowNotFoundException_WhenCodeInvalid()
        {
            // Arrange
            _mockGroupRepo.Setup(r => r.GetQueryable()).Returns(new List<NhomHocTap>().AsQueryable());
            var request = new JoinGroupRequest { MaThamGia = "INVALID" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.JoinGroupViaCodeAsync(userId: 1, request));
        }

        [Fact]
        public async Task JoinGroupViaCodeAsync_ShouldAddUser_WhenCodeValid()
        {
            // Arrange
            var userId = 2;
            var group = new NhomHocTap
            {
                MaNhom = 10,
                TenNhom = "Database Team",
                MaThamGia = "ABC123",
                SoLuongToiDa = 5,
                TrangThai = 1
            };

            _mockGroupRepo.Setup(r => r.GetQueryable()).Returns(new List<NhomHocTap> { group }.AsQueryable());
            _mockMemberRepo.Setup(r => r.AddAsync(It.IsAny<ThanhVienNhom>())).Returns(Task.CompletedTask);

            var request = new JoinGroupRequest { MaThamGia = "ABC123" };

            // Act
            var result = await _service.JoinGroupViaCodeAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.MaNhom);
            Assert.True(result.IsMember);

            _mockMemberRepo.Verify(r => r.AddAsync(It.Is<ThanhVienNhom>(m => m.MaNguoiDung == userId && m.MaNhom == 10)), Times.Once);
        }

        [Fact]
        public async Task LeaveGroupAsync_ShouldThrowBadRequestException_WhenOwnerTriesToLeaveWithOtherMembers()
        {
            // Arrange
            var ownerId = 1;
            var groupId = 5;

            var members = new List<ThanhVienNhom>
            {
                new ThanhVienNhom { MaNhom = groupId, MaNguoiDung = ownerId, VaiTro = 2, TrangThai = 1 },
                new ThanhVienNhom { MaNhom = groupId, MaNguoiDung = 2, VaiTro = 0, TrangThai = 1 }
            }.AsQueryable();

            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(members);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.LeaveGroupAsync(groupId, ownerId));
        }

        [Fact]
        public async Task RemoveMemberAsync_ShouldThrowUnauthorizedException_WhenUserIsNotAdmin()
        {
            // Arrange
            var regularUserId = 3;
            var groupId = 5;

            var members = new List<ThanhVienNhom>
            {
                new ThanhVienNhom { MaNhom = groupId, MaNguoiDung = regularUserId, VaiTro = 0, TrangThai = 1 }
            }.AsQueryable();

            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(members);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => 
                _service.RemoveMemberAsync(groupId, memberUserId: 2, currentUserId: regularUserId));
        }
    }
}
