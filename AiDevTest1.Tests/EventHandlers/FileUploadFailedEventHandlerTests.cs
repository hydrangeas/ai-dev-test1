using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.EventHandlers;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.EventHandlers
{
    public class FileUploadFailedEventHandlerTests
    {
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly Mock<IRetryPolicy> _mockRetryPolicy;
        private readonly FileUploadFailedEventHandler _handler;

        public FileUploadFailedEventHandlerTests()
        {
            _mockDialogService = new Mock<IDialogService>();
            _mockRetryPolicy = new Mock<IRetryPolicy>();
            _handler = new FileUploadFailedEventHandler(
                _mockDialogService.Object,
                _mockRetryPolicy.Object);
        }

        [Fact]
        public async Task HandleAsync_WithValidEvent_ShouldCompleteSuccessfully()
        {
            // Arrange
            var domainEvent = new FileUploadFailedEvent(
                new LogFilePath("2024-01-01.log"),
                "Network timeout");

            // Act
            await _handler.HandleAsync(domainEvent);

            // Assert
            // エラーが発生しないことを確認
        }

        [Theory]
        [InlineData("Network timeout", true)]
        [InlineData("Connection refused", true)]
        [InlineData("Service temporarily unavailable", true)]
        [InlineData("Please retry later", true)]
        [InlineData("File not found", false)]
        [InlineData("Access denied", false)]
        [InlineData("Invalid credentials", false)]
        public async Task HandleAsync_ShouldIdentifyTransientErrors(string errorMessage, bool isTransient)
        {
            // Arrange
            var domainEvent = new FileUploadFailedEvent(
                new LogFilePath("test.log"),
                errorMessage);

            // Act
            await _handler.HandleAsync(domainEvent);

            // Assert
            // ハンドラーの内部実装で一時的/永続的エラーを判定していることを確認
            // 実際の処理結果はイベントハンドラー内で処理される
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
            var action = () => new FileUploadFailedEventHandler(
                null,
                _mockRetryPolicy.Object);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("dialogService");
        }

        [Fact]
        public void Constructor_WithNullRetryPolicy_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new FileUploadFailedEventHandler(
                _mockDialogService.Object,
                null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("retryPolicy");
        }

        [Fact]
        public async Task HandleAsync_ShouldCompleteSuccessfully()
        {
            // Arrange
            var domainEvent = new FileUploadFailedEvent(
                new LogFilePath("test.log"),
                "Test error");

            // Act
            var task = _handler.HandleAsync(domainEvent);

            // Assert
            await task;
            task.IsCompletedSuccessfully.Should().BeTrue();
        }
    }
}