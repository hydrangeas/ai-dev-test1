using System;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.WpfApp.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.Services
{
    public class PolicyBasedDialogServiceTests
    {
        private readonly Mock<IDialogService> _mockInnerDialogService;
        private readonly Mock<IDialogDisplayPolicy> _mockDisplayPolicy;
        private readonly PolicyBasedDialogService _service;

        public PolicyBasedDialogServiceTests()
        {
            _mockInnerDialogService = new Mock<IDialogService>();
            _mockDisplayPolicy = new Mock<IDialogDisplayPolicy>();
            _service = new PolicyBasedDialogService(_mockInnerDialogService.Object, _mockDisplayPolicy.Object);
        }

        [Fact]
        public void Constructor_WithNullInnerDialogService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new PolicyBasedDialogService(null, _mockDisplayPolicy.Object);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("innerDialogService");
        }

        [Fact]
        public void Constructor_WithNullDisplayPolicy_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new PolicyBasedDialogService(_mockInnerDialogService.Object, null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("displayPolicy");
        }

        [Fact]
        public void ShowSuccess_ShouldCallDisplayPolicyWithCorrectParameters()
        {
            // Arrange
            var message = "Success message";
            _mockDisplayPolicy
                .Setup(x => x.DisplaySuccessAsync(message, _mockInnerDialogService.Object))
                .Returns(Task.CompletedTask);

            // Act
            _service.ShowSuccess(message);

            // Assert
            _mockDisplayPolicy.Verify(x => x.DisplaySuccessAsync(message, _mockInnerDialogService.Object), Times.Once);
        }

        [Fact]
        public void ShowError_ShouldCallDisplayPolicyWithCorrectParameters()
        {
            // Arrange
            var message = "Error message";
            _mockDisplayPolicy
                .Setup(x => x.DisplayErrorAsync(message, _mockInnerDialogService.Object))
                .Returns(Task.CompletedTask);

            // Act
            _service.ShowError(message);

            // Assert
            _mockDisplayPolicy.Verify(x => x.DisplayErrorAsync(message, _mockInnerDialogService.Object), Times.Once);
        }

        [Fact]
        public void ShowWarning_ShouldCallDisplayPolicyWithCorrectParameters()
        {
            // Arrange
            var message = "Warning message";
            _mockDisplayPolicy
                .Setup(x => x.DisplayWarningAsync(message, _mockInnerDialogService.Object))
                .Returns(Task.CompletedTask);

            // Act
            _service.ShowWarning(message);

            // Assert
            _mockDisplayPolicy.Verify(x => x.DisplayWarningAsync(message, _mockInnerDialogService.Object), Times.Once);
        }

        [Fact]
        public void ShowInfo_ShouldCallDisplayPolicyWithCorrectParameters()
        {
            // Arrange
            var message = "Info message";
            _mockDisplayPolicy
                .Setup(x => x.DisplayInfoAsync(message, _mockInnerDialogService.Object))
                .Returns(Task.CompletedTask);

            // Act
            _service.ShowInfo(message);

            // Assert
            _mockDisplayPolicy.Verify(x => x.DisplayInfoAsync(message, _mockInnerDialogService.Object), Times.Once);
        }

        [Fact]
        public void PolicyBasedDialogService_ShouldImplementIDialogService()
        {
            // Assert
            _service.Should().BeAssignableTo<IDialogService>();
        }

        [Fact]
        public void ShowSuccess_WhenPolicyThrowsException_ShouldPropagateException()
        {
            // Arrange
            var message = "Test message";
            var expectedException = new InvalidOperationException("Policy error");
            _mockDisplayPolicy
                .Setup(x => x.DisplaySuccessAsync(message, _mockInnerDialogService.Object))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var action = () => _service.ShowSuccess(message);
            action.Should().Throw<AggregateException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("Policy error");
        }
    }
}