using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Infrastructure.Policies;
using FluentAssertions;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.Policies
{
    public class StandardDialogDisplayPolicyTests
    {
        private readonly StandardDialogDisplayPolicy _policy;
        private readonly Mock<IDialogService> _mockDialogService;

        public StandardDialogDisplayPolicyTests()
        {
            _policy = new StandardDialogDisplayPolicy();
            _mockDialogService = new Mock<IDialogService>();
        }

        [Fact]
        public async Task DisplaySuccessAsync_ShouldCallShowSuccess()
        {
            // Arrange
            var message = "Success message";

            // Act
            await _policy.DisplaySuccessAsync(message, _mockDialogService.Object);

            // Assert
            _mockDialogService.Verify(x => x.ShowSuccess(message), Times.Once);
        }

        [Fact]
        public async Task DisplayErrorAsync_ShouldCallShowError()
        {
            // Arrange
            var message = "Error message";

            // Act
            await _policy.DisplayErrorAsync(message, _mockDialogService.Object);

            // Assert
            _mockDialogService.Verify(x => x.ShowError(message), Times.Once);
        }

        [Fact]
        public async Task DisplayWarningAsync_ShouldCallShowWarning()
        {
            // Arrange
            var message = "Warning message";

            // Act
            await _policy.DisplayWarningAsync(message, _mockDialogService.Object);

            // Assert
            _mockDialogService.Verify(x => x.ShowWarning(message), Times.Once);
        }

        [Fact]
        public async Task DisplayInfoAsync_ShouldCallShowInfo()
        {
            // Arrange
            var message = "Info message";

            // Act
            await _policy.DisplayInfoAsync(message, _mockDialogService.Object);

            // Assert
            _mockDialogService.Verify(x => x.ShowInfo(message), Times.Once);
        }

        [Fact]
        public void StandardDialogDisplayPolicy_ShouldImplementIDialogDisplayPolicy()
        {
            // Assert
            _policy.Should().BeAssignableTo<IDialogDisplayPolicy>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task DisplaySuccessAsync_WithEmptyOrNullMessage_ShouldStillCallShowSuccess(string message)
        {
            // Act
            await _policy.DisplaySuccessAsync(message, _mockDialogService.Object);

            // Assert
            _mockDialogService.Verify(x => x.ShowSuccess(message), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task DisplayErrorAsync_WithEmptyOrNullMessage_ShouldStillCallShowError(string message)
        {
            // Act
            await _policy.DisplayErrorAsync(message, _mockDialogService.Object);

            // Assert
            _mockDialogService.Verify(x => x.ShowError(message), Times.Once);
        }
    }
}