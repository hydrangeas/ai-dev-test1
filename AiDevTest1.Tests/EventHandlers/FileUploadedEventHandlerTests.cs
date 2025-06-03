using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.EventHandlers;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.EventHandlers
{
    public class FileUploadedEventHandlerTests
    {
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly FileUploadedEventHandler _handler;

        public FileUploadedEventHandlerTests()
        {
            _mockDialogService = new Mock<IDialogService>();
            _handler = new FileUploadedEventHandler(_mockDialogService.Object);
        }

        [Fact]
        public async Task HandleAsync_WithValidEvent_ShouldCompleteSuccessfully()
        {
            // Arrange
            var domainEvent = new FileUploadedEvent(
                new LogFilePath("2024-01-01.log"),
                new BlobName("logs/2024/01/01/device-001.log"),
                "blob://logs/2024/01/01/device-001.log");

            // Act
            await _handler.HandleAsync(domainEvent);

            // Assert
            // エラーが発生しないことを確認
        }

        [Fact]
        public void HandleAsync_WithNullEvent_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Func<Task> act = async () => await _handler.HandleAsync(null);
            act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("domainEvent");
        }

        [Fact]
        public void Constructor_WithNullDialogService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new FileUploadedEventHandler(null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("dialogService");
        }

        [Fact]
        public async Task HandleAsync_ShouldCompleteSuccessfully()
        {
            // Arrange
            var domainEvent = new FileUploadedEvent(
                new LogFilePath("test.log"),
                new BlobName("test-blob"),
                "blob://test-blob");

            // Act
            var task = _handler.HandleAsync(domainEvent);

            // Assert
            await task;
            task.IsCompletedSuccessfully.Should().BeTrue();
        }
    }
}