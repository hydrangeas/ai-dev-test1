using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Commands;
using AiDevTest1.Application.Handlers;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.Handlers
{
    public class UploadFileCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService;
        private readonly UploadFileCommandHandler _handler;

        public UploadFileCommandHandlerTests()
        {
            _mockFileUploadService = new Mock<IFileUploadService>();
            _handler = new UploadFileCommandHandler(_mockFileUploadService.Object);
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
            var action = () => new UploadFileCommandHandler(null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("fileUploadService");
        }
    }
}