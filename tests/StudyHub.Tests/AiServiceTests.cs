using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudyHub.Application.Common.Interfaces.Services;
using StudyHub.Application.DTOs.Ai;
using StudyHub.Application.DTOs.Dashboard;
using StudyHub.Infrastructure.Services;
using Xunit;

namespace StudyHub.Tests
{
    public class AiServiceTests
    {
        private readonly Mock<IDashboardService> _mockDashboardService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AiService>> _mockLogger;
        private readonly HttpClient _httpClient;
        private readonly AiService _service;

        public AiServiceTests()
        {
            _mockDashboardService = new Mock<IDashboardService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AiService>>();
            _httpClient = new HttpClient();

            _service = new AiService(
                _httpClient,
                _mockDashboardService.Object,
                _mockConfiguration.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task ChatAsync_ShouldBuildUserContextAndReturnReply()
        {
            // Arrange
            var userId = 1;
            _mockDashboardService.Setup(d => d.GetDashboardDataAsync(userId))
                .ReturnsAsync(new DashboardDto
                {
                    UserProfile = new DashboardUserProfileDto { HoTen = "Test Student" },
                    Statistics = new DashboardStatisticsDto { TongSoCongViec = 5, CongViecChuaHoanThanh = 2 }
                });

            var request = new AiChatRequest { Message = "Hôm nay nên học gì?" };

            // Act
            var response = await _service.ChatAsync(userId, request);

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response.Reply);
            Assert.NotEmpty(response.ActionSuggestions);
        }

        [Fact]
        public async Task GenerateStudyPlanAsync_ShouldReturnStructuredPlan()
        {
            // Arrange
            var userId = 1;
            _mockDashboardService.Setup(d => d.GetDashboardDataAsync(userId))
                .ReturnsAsync(new DashboardDto
                {
                    UserProfile = new DashboardUserProfileDto { HoTen = "Test Student" },
                    Statistics = new DashboardStatisticsDto { TongSoMonHoc = 3 }
                });

            var request = new StudyPlanRequest { Goal = "Ôn thi Toán cao cấp", NumberOfDays = 7 };

            // Act
            var result = await _service.GenerateStudyPlanAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7, result.PlanItems.Count);
            Assert.Contains("Ôn thi Toán cao cấp", result.Title);
        }

        [Fact]
        public async Task AnalyzeWorkloadAsync_ShouldCalculateWorkloadLevel()
        {
            // Arrange
            var userId = 1;
            _mockDashboardService.Setup(d => d.GetDashboardDataAsync(userId))
                .ReturnsAsync(new DashboardDto
                {
                    Statistics = new DashboardStatisticsDto { TongSoCongViec = 12, CongViecChuaHoanThanh = 9, DeadlineHomNay = 3 }
                });

            // Act
            var result = await _service.AnalyzeWorkloadAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("QUÁ TẢI CAO", result);
        }
    }
}
