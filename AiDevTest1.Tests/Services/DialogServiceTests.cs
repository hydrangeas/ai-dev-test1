using AiDevTest1.Application.Interfaces;
using AiDevTest1.WpfApp.Services;
using FluentAssertions;
using Xunit;

namespace AiDevTest1.Tests.Services
{
    public class DialogServiceTests
    {
        private readonly DialogService _dialogService;

        public DialogServiceTests()
        {
            _dialogService = new DialogService();
        }

        [Fact]
        public void ShowSuccess_ShouldNotThrow()
        {
            // Arrange
            var message = "Success message";

            // Act & Assert
            var action = () => _dialogService.ShowSuccess(message);
            action.Should().NotThrow();
        }

        [Fact]
        public void ShowError_ShouldNotThrow()
        {
            // Arrange
            var message = "Error message";

            // Act & Assert
            var action = () => _dialogService.ShowError(message);
            action.Should().NotThrow();
        }

        [Fact]
        public void ShowWarning_ShouldNotThrow()
        {
            // Arrange
            var message = "Warning message";

            // Act & Assert
            var action = () => _dialogService.ShowWarning(message);
            action.Should().NotThrow();
        }

        [Fact]
        public void ShowInfo_ShouldNotThrow()
        {
            // Arrange
            var message = "Info message";

            // Act & Assert
            var action = () => _dialogService.ShowInfo(message);
            action.Should().NotThrow();
        }

        [Fact]
        public void DialogService_ShouldImplementIDialogService()
        {
            // Assert
            _dialogService.Should().BeAssignableTo<IDialogService>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ShowSuccess_WithEmptyOrNullMessage_ShouldNotThrow(string message)
        {
            // Act & Assert
            var action = () => _dialogService.ShowSuccess(message);
            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ShowError_WithEmptyOrNullMessage_ShouldNotThrow(string message)
        {
            // Act & Assert
            var action = () => _dialogService.ShowError(message);
            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ShowWarning_WithEmptyOrNullMessage_ShouldNotThrow(string message)
        {
            // Act & Assert
            var action = () => _dialogService.ShowWarning(message);
            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ShowInfo_WithEmptyOrNullMessage_ShouldNotThrow(string message)
        {
            // Act & Assert
            var action = () => _dialogService.ShowInfo(message);
            action.Should().NotThrow();
        }
    }
}