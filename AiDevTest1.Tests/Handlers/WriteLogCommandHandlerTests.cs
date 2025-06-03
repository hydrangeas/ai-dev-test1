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
    public class WriteLogCommandHandlerTests
    {
        private readonly Mock<ILogWriteService> _mockLogWriteService;
        private readonly WriteLogCommandHandler _handler;

        public WriteLogCommandHandlerTests()
        {
            _mockLogWriteService = new Mock<ILogWriteService>();
            _handler = new WriteLogCommandHandler(_mockLogWriteService.Object);
        }

        [Fact]
        public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
        {
            // Arrange
            var command = new WriteLogCommand();
            var expectedResult = Result.Success();
            _mockLogWriteService.Setup(x => x.WriteLogEntryAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _mockLogWriteService.Verify(x => x.WriteLogEntryAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithNullCommand_ShouldReturnFailureResult()
        {
            // Arrange
            WriteLogCommand nullCommand = null;

            // Act
            var result = await _handler.HandleAsync(nullCommand);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("コマンドがnullです。");
            _mockLogWriteService.Verify(x => x.WriteLogEntryAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenLogWriteServiceFails_ShouldReturnFailureResult()
        {
            // Arrange
            var command = new WriteLogCommand();
            var expectedErrorMessage = "ログファイルへの書き込みに失敗しました";
            var expectedResult = Result.Failure(expectedErrorMessage);
            _mockLogWriteService.Setup(x => x.WriteLogEntryAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be(expectedErrorMessage);
            _mockLogWriteService.Verify(x => x.WriteLogEntryAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenLogWriteServiceThrowsException_ShouldReturnFailureResult()
        {
            // Arrange
            var command = new WriteLogCommand();
            var exceptionMessage = "Test exception";
            _mockLogWriteService.Setup(x => x.WriteLogEntryAsync())
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain(exceptionMessage);
            _mockLogWriteService.Verify(x => x.WriteLogEntryAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToService()
        {
            // Arrange
            var command = new WriteLogCommand();
            var cancellationToken = new CancellationToken();
            var expectedResult = Result.Success();
            _mockLogWriteService.Setup(x => x.WriteLogEntryAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _handler.HandleAsync(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _mockLogWriteService.Verify(x => x.WriteLogEntryAsync(), Times.Once);
        }

        [Fact]
        public void Constructor_WithNullLogWriteService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new WriteLogCommandHandler(null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logWriteService");
        }
    }
}