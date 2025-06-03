using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.Interfaces;
using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.ValueObjects;
using AiDevTest1.Infrastructure.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.Infrastructure
{
    public class EventDispatcherTests
    {
        private readonly Mock<ILogger<EventDispatcher>> _mockLogger;

        public EventDispatcherTests()
        {
            _mockLogger = new Mock<ILogger<EventDispatcher>>();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenServiceProviderIsNull()
        {
            Action act = () => new EventDispatcher(null, _mockLogger.Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("serviceProvider");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            Action act = () => new EventDispatcher(serviceProvider, null);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Fact]
        public async Task DispatchAsync_ShouldThrowArgumentNullException_WhenEventIsNull()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            Func<Task> act = async () => await dispatcher.DispatchAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("domainEvent");
        }

        [Fact]
        public async Task DispatchAsync_ShouldCallSingleHandler()
        {
            // Arrange
            var mockHandler = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            await dispatcher.DispatchAsync(domainEvent);

            // Assert
            mockHandler.Verify(x => x.HandleAsync(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DispatchAsync_ShouldCallMultipleHandlers()
        {
            // Arrange
            var mockHandler1 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler1.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockHandler2 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler2.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler1.Object);
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler2.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            await dispatcher.DispatchAsync(domainEvent);

            // Assert
            mockHandler1.Verify(x => x.HandleAsync(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
            mockHandler2.Verify(x => x.HandleAsync(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DispatchAsync_ShouldNotThrow_WhenNoHandlersRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            Func<Task> act = async () => await dispatcher.DispatchAsync(domainEvent);

            // Assert
            await act.Should().NotThrowAsync();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No handlers found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DispatchAsync_ShouldWaitForAllHandlers()
        {
            // Arrange
            var completionOrder = new System.Collections.Concurrent.ConcurrentBag<int>();

            var mockHandler1 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler1.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(100);
                    completionOrder.Add(1);
                });

            var mockHandler2 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler2.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(50);
                    completionOrder.Add(2);
                });

            var services = new ServiceCollection();
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler1.Object);
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler2.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            await dispatcher.DispatchAsync(domainEvent);

            // Assert
            completionOrder.Should().HaveCount(2);
            completionOrder.Should().Contain(new[] { 1, 2 });
        }

        [Fact]
        public async Task DispatchAsync_ShouldThrowAggregateException_WhenHandlerThrows()
        {
            // Arrange
            var mockHandler = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Handler failed"));

            var services = new ServiceCollection();
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            Func<Task> act = async () => await dispatcher.DispatchAsync(domainEvent);

            // Assert
            await act.Should().ThrowAsync<AggregateException>()
                .WithMessage("*One or more handlers failed*");
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handler") && v.ToString().Contains("failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task DispatchAsync_ShouldThrowAggregateException_WhenMultipleHandlersFail()
        {
            // Arrange
            var mockHandler1 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler1.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Handler 1 failed"));

            var mockHandler2 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler2.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Handler 2 failed"));

            var services = new ServiceCollection();
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler1.Object);
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler2.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            Func<Task> act = async () => await dispatcher.DispatchAsync(domainEvent);

            // Assert
            await act.Should().ThrowAsync<AggregateException>();
        }

        [Fact]
        public async Task DispatchAsync_ShouldRespectCancellationToken()
        {
            // Arrange
            var tcs = new TaskCompletionSource<bool>();
            var mockHandler = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .Returns(async (LogWrittenToFileEvent e, CancellationToken ct) =>
                {
                    await Task.Delay(1000, ct);
                    tcs.SetResult(true);
                });

            var services = new ServiceCollection();
            services.AddSingleton<IEventHandler<LogWrittenToFileEvent>>(mockHandler.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(100);

            // Act
            Func<Task> act = async () => await dispatcher.DispatchAsync(domainEvent, cts.Token);

            // Assert
            await act.Should().ThrowAsync<AggregateException>()
                .WithInnerException<OperationCanceledException>();
            tcs.Task.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task DispatchAsync_ShouldHandleInheritedEventTypes()
        {
            // Arrange
            var mockHandler = new Mock<IEventHandler<FileUploadedEvent>>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<FileUploadedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton<IEventHandler<FileUploadedEvent>>(mockHandler.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var deviceId = DeviceId.Create("device123").Value;
            var blobName = BlobName.Create("test.log").Value;
            var domainEvent = new FileUploadedEvent(deviceId, blobName);

            // Act
            await dispatcher.DispatchAsync(domainEvent);

            // Assert
            mockHandler.Verify(x => x.HandleAsync(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DispatchAsync_ShouldUseScopedServiceProvider()
        {
            // Arrange
            var mockHandler = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddScoped<IEventHandler<LogWrittenToFileEvent>>(sp => mockHandler.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider, _mockLogger.Object);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            await dispatcher.DispatchAsync(domainEvent);

            // Assert
            mockHandler.Verify(x => x.HandleAsync(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}