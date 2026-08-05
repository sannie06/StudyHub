using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.DTOs.TaiLieu;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class TaiLieuServiceTests : IDisposable
    {
        private readonly Mock<ITaiLieuRepository> _mockTaiLieuRepo;
        private readonly Mock<IFileTaiLenRepository> _mockFileRepo;
        private readonly Mock<IGenericRepository<ThanhVienNhom>> _mockMemberRepo;
        private readonly Mock<IGenericRepository<NhomHocTap>> _mockGroupRepo;
        private readonly Mock<IGenericRepository<NguoiDung>> _mockUserRepo;
        private readonly Mock<ILogger<TaiLieuService>> _mockLogger;
        private readonly TaiLieuService _taiLieuService;
        private readonly string _testUploadsFolder;

        public TaiLieuServiceTests()
        {
            _mockTaiLieuRepo = new Mock<ITaiLieuRepository>();
            _mockFileRepo = new Mock<IFileTaiLenRepository>();
            _mockMemberRepo = new Mock<IGenericRepository<ThanhVienNhom>>();
            _mockGroupRepo = new Mock<IGenericRepository<NhomHocTap>>();
            _mockUserRepo = new Mock<IGenericRepository<NguoiDung>>();
            _mockLogger = new Mock<ILogger<TaiLieuService>>();

            _taiLieuService = new TaiLieuService(
                _mockTaiLieuRepo.Object,
                _mockFileRepo.Object,
                _mockMemberRepo.Object,
                _mockGroupRepo.Object,
                _mockUserRepo.Object,
                _mockLogger.Object
            );

            // Prepare local folder for physical file test
            _testUploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
            if (!Directory.Exists(_testUploadsFolder))
            {
                Directory.CreateDirectory(_testUploadsFolder);
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_testUploadsFolder))
            {
                try
                {
                    Directory.Delete(_testUploadsFolder, true);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }

        [Fact]
        public async Task GetDocumentsAsync_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
        {
            // Arrange
            _mockGroupRepo.Setup(r => r.GetQueryable())
                .Returns(new List<NhomHocTap>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _taiLieuService.GetDocumentsAsync(userId: 1, maNhom: 99, search: null));
        }

        [Fact]
        public async Task GetDocumentsAsync_ShouldThrowUnauthorizedException_WhenUserIsNotMember()
        {
            // Arrange
            var groups = new List<NhomHocTap> { new NhomHocTap { MaNhom = 1 } }.AsQueryable();
            var members = new List<ThanhVienNhom>().AsQueryable(); // Empty members

            _mockGroupRepo.Setup(r => r.GetQueryable()).Returns(groups);
            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(members);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => 
                _taiLieuService.GetDocumentsAsync(userId: 1, maNhom: 1, search: null));
        }

        [Fact]
        public async Task GetDocumentsAsync_ShouldReturnDocuments_WhenUserIsMember()
        {
            // Arrange
            var userId = 1;
            var maNhom = 1;

            var groups = new List<NhomHocTap> { new NhomHocTap { MaNhom = maNhom } }.AsQueryable();
            var members = new List<ThanhVienNhom> { new ThanhVienNhom { MaNhom = maNhom, MaNguoiDung = userId } }.AsQueryable();
            
            var file = new FileTaiLen { MaFile = 10, TenGoc = "math.pdf", LoaiFile = "application/pdf" };
            var uploader = new NguoiDung { MaNguoiDung = userId, HoTen = "John Doe" };
            var documents = new List<TaiLieu>
            {
                new TaiLieu { MaTaiLieu = 5, MaNhom = maNhom, MaFile = 10, FileTaiLen = file, NguoiTaiLen = uploader, TieuDe = "Math Textbook", MoTa = "Reference textbook" }
            }.AsQueryable();

            _mockGroupRepo.Setup(r => r.GetQueryable()).Returns(groups);
            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(members);
            _mockTaiLieuRepo.Setup(r => r.GetQueryable()).Returns(documents);

            // Act
            var result = await _taiLieuService.GetDocumentsAsync(userId, maNhom, search: "Textbook");

            // Assert
            Assert.Single(result);
            var docDto = result.First();
            Assert.Equal("Math Textbook", docDto.TieuDe);
            Assert.Equal("John Doe", docDto.TenNguoiTaiLen);
            Assert.Equal("math.pdf", docDto.TenGoc);
        }

        [Fact]
        public async Task UploadDocumentAsync_ShouldCreateRecords_WhenValidRequest()
        {
            // Arrange
            var userId = 1;
            var maNhom = 1;
            var groups = new List<NhomHocTap> { new NhomHocTap { MaNhom = maNhom } }.AsQueryable();
            var members = new List<ThanhVienNhom> { new ThanhVienNhom { MaNhom = maNhom, MaNguoiDung = userId } }.AsQueryable();
            var users = new List<NguoiDung> { new NguoiDung { MaNguoiDung = userId, HoTen = "John Doe" } }.AsQueryable();

            _mockGroupRepo.Setup(r => r.GetQueryable()).Returns(groups);
            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(members);
            _mockUserRepo.Setup(r => r.GetQueryable()).Returns(users);

            var fileContent = "Fake PDF File Content";
            var bytes = Encoding.UTF8.GetBytes(fileContent);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(bytes.Length);
            mockFile.Setup(f => f.FileName).Returns("math_hw.pdf");
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => stream.Write(bytes, 0, bytes.Length))
                .Returns(Task.CompletedTask);

            var request = new UploadDocumentRequest
            {
                MaNhom = maNhom,
                TieuDe = "Math HW 1",
                MoTa = "Week 1 homework",
                File = mockFile.Object
            };

            _mockFileRepo.Setup(r => r.AddAsync(It.IsAny<FileTaiLen>())).Returns(Task.CompletedTask);
            _mockTaiLieuRepo.Setup(r => r.AddAsync(It.IsAny<TaiLieu>())).Returns(Task.CompletedTask);

            // Act
            var result = await _taiLieuService.UploadDocumentAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Math HW 1", result.TieuDe);
            Assert.Equal("math_hw.pdf", result.TenGoc);
            Assert.Equal("John Doe", result.TenNguoiTaiLen);

            _mockFileRepo.Verify(r => r.AddAsync(It.IsAny<FileTaiLen>()), Times.Once);
            _mockTaiLieuRepo.Verify(r => r.AddAsync(It.IsAny<TaiLieu>()), Times.Once);
        }

        [Fact]
        public async Task UpdateDocumentAsync_ShouldThrowUnauthorizedException_WhenUserIsNotUploader()
        {
            // Arrange
            var uploaderId = 2;
            var editorId = 3;
            var document = new TaiLieu { MaTaiLieu = 1, MaNguoiTaiLen = uploaderId };
            var query = new List<TaiLieu> { document }.AsQueryable();

            _mockTaiLieuRepo.Setup(r => r.GetQueryable()).Returns(query);

            var request = new UpdateDocumentRequest { TieuDe = "Updated Title", MoTa = "Updated Desc" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => 
                _taiLieuService.UpdateDocumentAsync(id: 1, userId: editorId, request: request));
        }

        [Fact]
        public async Task DownloadDocumentAsync_ShouldReturnFileBytes_AndIncreaseDownloadCount()
        {
            // Arrange
            var userId = 1;
            var maNhom = 1;
            var testFileName = "test_guid.pdf";
            var testFilePath = Path.Combine(_testUploadsFolder, testFileName);
            
            await File.WriteAllTextAsync(testFilePath, "Actual File Bytes");

            var groups = new List<NhomHocTap> { new NhomHocTap { MaNhom = maNhom } }.AsQueryable();
            var members = new List<ThanhVienNhom> { new ThanhVienNhom { MaNhom = maNhom, MaNguoiDung = userId } }.AsQueryable();

            var fileRecord = new FileTaiLen { MaFile = 10, TenGoc = "lecture1.pdf", TenLuu = testFileName, LoaiFile = "application/pdf" };
            var document = new TaiLieu { MaTaiLieu = 5, MaNhom = maNhom, MaFile = 10, FileTaiLen = fileRecord, LuotTai = 2 };
            var query = new List<TaiLieu> { document }.AsQueryable();

            _mockGroupRepo.Setup(r => r.GetQueryable()).Returns(groups);
            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(members);
            _mockTaiLieuRepo.Setup(r => r.GetQueryable()).Returns(query);

            // Act
            var (fileStream, contentType, fileName) = await _taiLieuService.DownloadDocumentAsync(5, userId);
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            fileStream.Dispose();
            var fileBytes = ms.ToArray();

            // Assert
            Assert.Equal("Actual File Bytes", Encoding.UTF8.GetString(fileBytes));
            Assert.Equal("application/pdf", contentType);
            Assert.Equal("lecture1.pdf", fileName);
            Assert.Equal(3, document.LuotTai); // Incremented from 2 to 3

            _mockTaiLieuRepo.Verify(r => r.Update(document), Times.Once);
        }

        [Fact]
        public async Task GetMyGroupsAsync_ShouldReturnGroups_WhenUserIsMember()
        {
            // Arrange
            var userId = 1;
            var group = new NhomHocTap { MaNhom = 2, TenNhom = "Algorithms Group" };
            var members = new List<ThanhVienNhom> 
            { 
                new ThanhVienNhom { MaNguoiDung = userId, MaNhom = 2, NhomHocTap = group } 
            }.AsQueryable();

            _mockMemberRepo.Setup(r => r.GetQueryable()).Returns(members);

            // Act
            var result = await _taiLieuService.GetMyGroupsAsync(userId);

            // Assert
            Assert.Single(result);
            var gDto = result.First();
            Assert.Equal(2, gDto.MaNhom);
            Assert.Equal("Algorithms Group", gDto.TenNhom);
        }
    }
}
