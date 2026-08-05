using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class DashboardServiceTests
    {
        private readonly Mock<IGenericRepository<NguoiDung>> _mockUserRepo;
        private readonly Mock<IGenericRepository<MonHoc>> _mockSubjectRepo;
        private readonly Mock<IGenericRepository<CongViec>> _mockTaskRepo;
        private readonly Mock<ILichHocRepository> _mockClassRepo;
        private readonly Mock<ILichThiRepository> _mockExamRepo;
        private readonly Mock<INhomHocTapRepository> _mockGroupRepo;
        private readonly Mock<IThanhVienNhomRepository> _mockGroupMemberRepo;
        private readonly Mock<ITaiLieuRepository> _mockDocRepo;
        private readonly Mock<IThongBaoRepository> _mockNotifRepo;
        private readonly DashboardService _service;

        public DashboardServiceTests()
        {
            _mockUserRepo = new Mock<IGenericRepository<NguoiDung>>();
            _mockSubjectRepo = new Mock<IGenericRepository<MonHoc>>();
            _mockTaskRepo = new Mock<IGenericRepository<CongViec>>();
            _mockClassRepo = new Mock<ILichHocRepository>();
            _mockExamRepo = new Mock<ILichThiRepository>();
            _mockGroupRepo = new Mock<INhomHocTapRepository>();
            _mockGroupMemberRepo = new Mock<IThanhVienNhomRepository>();
            _mockDocRepo = new Mock<ITaiLieuRepository>();
            _mockNotifRepo = new Mock<IThongBaoRepository>();

            _service = new DashboardService(
                _mockUserRepo.Object,
                _mockSubjectRepo.Object,
                _mockTaskRepo.Object,
                _mockClassRepo.Object,
                _mockExamRepo.Object,
                _mockGroupRepo.Object,
                _mockGroupMemberRepo.Object,
                _mockDocRepo.Object,
                _mockNotifRepo.Object
            );
        }

        [Fact]
        public async Task GetDashboardDataAsync_ShouldReturnAggregatedRealDataForUser()
        {
            // Arrange
            var userId = 1;

            _mockUserRepo.Setup(r => r.GetQueryable()).Returns(new List<NguoiDung>
            {
                new NguoiDung { MaNguoiDung = userId, HoTen = "Nguyen Van A", Email = "test@studyhub.vn" }
            }.AsQueryable());

            _mockSubjectRepo.Setup(r => r.GetQueryable()).Returns(new List<MonHoc>
            {
                new MonHoc { MaMonHoc = 1, TenMonHoc = "Toan" }
            }.AsQueryable());

            _mockTaskRepo.Setup(r => r.GetQueryable()).Returns(new List<CongViec>
            {
                new CongViec { MaCongViec = 1, MaNguoiDung = userId, TieuDe = "Lam bai tap", TrangThai = 0 }
            }.AsQueryable());

            _mockClassRepo.Setup(r => r.GetQueryable()).Returns(new List<LichHoc>().AsQueryable());
            _mockExamRepo.Setup(r => r.GetQueryable()).Returns(new List<LichThi>().AsQueryable());
            _mockGroupMemberRepo.Setup(r => r.GetQueryable()).Returns(new List<ThanhVienNhom>().AsQueryable());
            _mockGroupRepo.Setup(r => r.GetQueryable()).Returns(new List<NhomHocTap>().AsQueryable());
            _mockDocRepo.Setup(r => r.GetQueryable()).Returns(new List<TaiLieu>().AsQueryable());
            _mockNotifRepo.Setup(r => r.GetQueryable()).Returns(new List<ThongBao>().AsQueryable());

            // Act
            var result = await _service.GetDashboardDataAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Nguyen Van A", result.UserProfile.HoTen);
            Assert.Equal(1, result.Statistics.TongSoMonHoc);
            Assert.Equal(1, result.Statistics.TongSoCongViec);
            Assert.Single(result.TodayTasks);
        }
    }
}
