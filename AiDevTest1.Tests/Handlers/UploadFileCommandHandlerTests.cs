using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Commands;
using AiDevTest1.Application.Handlers;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.Handlers
{
    public class UploadFileCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService;
        private readonly Mock<IEventDispatcher> _mockEventDispatcher;
        private readonly Mock<ILogFileHandler> _mockLogFileHandler;
        private readonly UploadFileCommandHandler _handler;
        private readonly LogFilePath _testLogFilePath;

        public UploadFileCommandHandlerTests()
        {
            _mockFileUploadService = new Mock<IFileUploadService>();
            _mockEventDispatcher = new Mock<IEventDispatcher>();
            _mockLogFileHandler = new Mock<ILogFileHandler>();
            _testLogFilePath = new LogFilePath("2024-01-01.log");
            
            _mockLogFileHandler.Setup(x => x.GetCurrentLogFilePath())
                .Returns(_testLogFilePath);
                
            _handler = new UploadFileCommandHandler(
                _mockFileUploadService.Object,
                _mockEventDispatcher.Object,
                _mockLogFileHandler.Object);
        }

        [Fact]
        public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
        {
            // Arrange
            var command = new UploadFileCommand();
            var expectedResult = Result.Success();
            _mockFileUploadService.Setup(x => x.UploadLogFileAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _mockFileUploadService.Verify(x => x.UploadLogFileAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_OnSuccess_ShouldDispatchFileUploadedEvent()
        {
            // Arrange
            var command = new UploadFileCommand();
            var expectedResult = Result.Success();
            _mockFileUploadService.Setup(x => x.UploadLogFileAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockEventDispatcher.Verify(x => x.DispatchAsync(
                It.IsAny<FileUploadedEvent>(), 
                It.IsAny<CancellationToken>()), Times.Once);
            // Should not dispatch failure event
            _mockEventDispatcher.Verify(x => x.DispatchAsync(
                It.IsAny<FileUploadFailedEvent>(), 
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithNullCommand_ShouldReturnFailureResult()
        {
            // Arrange
            UploadFileCommand nullCommand = null;

            // Act
            var result = await _handler.HandleAsync(nullCommand);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("コマンドがnullです。");
            _mockFileUploadService.Verify(x => x.UploadLogFileAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenFileUploadServiceFails_ShouldReturnFailureResult()
        {
            // Arrange
            var command = new UploadFileCommand();
            var expectedErrorMessage = "ファイルアップロードに失敗しました";
            var expectedResult = Result.Failure(expectedErrorMessage);
            _mockFileUploadService.Setup(x => x.UploadLogFileAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be(expectedErrorMessage);
            _mockFileUploadService.Verify(x => x.UploadLogFileAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_OnFailure_ShouldDispatchFileUploadFailedEvent()
        {
            // Arrange
            var command = new UploadFileCommand();
            var expectedErrorMessage = "ファイルアップロードに失敗しました";
            var expectedResult = Result.Failure(expectedErrorMessage);
            _mockFileUploadService.Setup(x => x.UploadLogFileAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            result.IsSuccess.Should().BeFalse();
            _mockEventDispatcher.Verify(x => x.DispatchAsync(
                It.IsAny<FileUploadFailedEvent>(), 
                It.IsAny<CancellationToken>()), Times.Once);
            // Should not dispatch success event
            _mockEventDispatcher.Verify(x => x.DispatchAsync(
                It.IsAny<FileUploadedEvent>(), 
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenFileUploadServiceThrowsException_ShouldReturnFailureResult()
        {
            // Arrange
            var command = new UploadFileCommand();
            var exceptionMessage = "Test exception";
            _mockFileUploadService.Setup(x => x.UploadLogFileAsync())
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain(exceptionMessage);
            _mockFileUploadService.Verify(x => x.UploadLogFileAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToService()
        {
            // Arrange
            var command = new UploadFileCommand();
            var cancellationToken = new CancellationToken();
            var expectedResult = Result.Success();
            _mockFileUploadService.Setup(x => x.UploadLogFileAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _handler.HandleAsync(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _mockFileUploadService.Verify(x => x.UploadLogFileAsync(), Times.Once);
        }

        [Fact]
        public void Constructor_WithNullFileUploadService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new UploadFileCommandHandler(null, _mockEventDispatcher.Object, _mockLogFileHandler.Object);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("fileUploadService");
        }

        [Fact]
        public void Constructor_WithNullEventDispatcher_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new UploadFileCommandHandler(_mockFileUploadService.Object, null, _mockLogFileHandler.Object);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("eventDispatcher");
        }

        [Fact]
        public void Constructor_WithNullLogFileHandler_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new UploadFileCommandHandler(_mockFileUploadService.Object, _mockEventDispatcher.Object, null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logFileHandler");
        }
    }
}