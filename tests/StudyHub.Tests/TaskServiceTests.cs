using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudyHub.Application.Common.Exceptions;
using StudyHub.Application.Common.Interfaces.Persistence;
using StudyHub.Application.DTOs.Task;
using StudyHub.Domain.Entities;
using StudyHub.Infrastructure.Services;
using Xunit;

using Microsoft.Extensions.Logging;

namespace StudyHub.Tests
{
    public class TaskServiceTests
    {
        private readonly Mock<IGenericRepository<CongViec>> _mockTaskRepo;
        private readonly Mock<IGenericRepository<MonHoc>> _mockSubjectRepo;
        private readonly Mock<ILogger<TaskService>> _mockLogger;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _mockTaskRepo = new Mock<IGenericRepository<CongViec>>();
            _mockSubjectRepo = new Mock<IGenericRepository<MonHoc>>();
            _mockLogger = new Mock<ILogger<TaskService>>();
            _taskService = new TaskService(_mockTaskRepo.Object, _mockSubjectRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldPopulateCompletedDate_WhenStatusIsCompleted()
        {
            // Arrange
            var userId = 1;
            var request = new CreateTaskRequest
            {
                TieuDe = "Test Task",
                DoUuTien = 1,
                TrangThai = 3, // Completed
                TiLeHoanThanh = 100
            };

            CongViec? savedTask = null;
            _mockTaskRepo.Setup(r => r.AddAsync(It.IsAny<CongViec>()))
                .Callback<CongViec>(t => savedTask = t)
                .Returns(Task.CompletedTask);

            // Mock detail query lookup inside TaskService
            var queryableTasks = new List<CongViec>().AsQueryable();
            _mockTaskRepo.Setup(r => r.GetQueryable())
                .Returns(() => {
                    var list = new List<CongViec>();
                    if (savedTask != null)
                    {
                        savedTask.MaCongViec = 99; // Mock DB auto-increment ID
                        list.Add(savedTask);
                    }
                    return list.AsQueryable();
                });

            // Act
            var result = await _taskService.CreateTaskAsync(userId, request);

            // Assert
            Assert.NotNull(savedTask);
            Assert.Equal(3, savedTask.TrangThai);
            Assert.NotNull(savedTask.NgayHoanThanh); // Automatically populated completed date
            Assert.True((DateTime.UtcNow - savedTask.NgayHoanThanh.Value).TotalSeconds < 5);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ShouldSetCompletedDateAndProgressTo100_WhenStatusIsCompleted()
        {
            // Arrange
            var taskId = 10;
            var userId = 1;
            var existingTask = new CongViec
            {
                MaCongViec = taskId,
                MaNguoiDung = userId,
                TieuDe = "Incomplete Task",
                TrangThai = 1, // In Progress
                TiLeHoanThanh = 40,
                NgayHoanThanh = null
            };

            _mockTaskRepo.Setup(r => r.GetQueryable())
                .Returns(new List<CongViec> { existingTask }.AsQueryable());

            // Act
            var result = await _taskService.UpdateTaskStatusAsync(taskId, userId, 3); // Update status to 3 (Completed)

            // Assert
            Assert.Equal(3, existingTask.TrangThai);
            Assert.Equal(100, existingTask.TiLeHoanThanh);
            Assert.NotNull(existingTask.NgayHoanThanh);
            _mockTaskRepo.Verify(r => r.Update(existingTask), Times.Once);
            _mockTaskRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockTaskRepo.Setup(r => r.GetQueryable())
                .Returns(new List<CongViec>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _taskService.UpdateTaskStatusAsync(99, 1, 3));
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldSuccessfullyDeleteTask()
        {
            // Arrange
            var taskId = 10;
            var userId = 1;
            var task = new CongViec { MaCongViec = taskId, MaNguoiDung = userId };

            _mockTaskRepo.Setup(r => r.GetQueryable())
                .Returns(new List<CongViec> { task }.AsQueryable());

            // Act
            await _taskService.DeleteTaskAsync(taskId, userId);

            // Assert
            _mockTaskRepo.Verify(r => r.Delete(task), Times.Once);
            _mockTaskRepo.Verify(r => r.SaveAsync(), Times.Once);
        }
    }
}
