using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.EventHandlers;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.EventHandlers
{
    public class LogWrittenToFileEventHandlerTests
    {
        private readonly LogWrittenToFileEventHandler _handler;

        public LogWrittenToFileEventHandlerTests()
        {
            _handler = new LogWrittenToFileEventHandler();
        }

        [Fact]
        public async Task HandleAsync_WithValidEvent_ShouldCompleteSuccessfully()
        {
            // Arrange
            var filePath = new LogFilePath("2024-01-01.log");
            var logEntry = new LogEntry(
                DateTime.Now,
                EventType.START,
                new DeviceId("device-001"));
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

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
        public async Task HandleAsync_ShouldCompleteSuccessfully()
        {
            // Arrange
            var filePath = new LogFilePath("test.log");
            var logEntry = new LogEntry(
                DateTime.Now,
                EventType.ERROR,
                new DeviceId("device-test"));
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            var task = _handler.HandleAsync(domainEvent);

            // Assert
            await task;
            task.IsCompletedSuccessfully.Should().BeTrue();
        }
    }
}