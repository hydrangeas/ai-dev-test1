using System;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.Interfaces;
using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.ValueObjects;
using AiDevTest1.Infrastructure.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.Infrastructure
{
    public class EventDispatcherTests
    {
        private readonly ServiceCollection _services;
        private readonly EventDispatcher _dispatcher;
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcherTests()
        {
            _services = new ServiceCollection();
            _serviceProvider = _services.BuildServiceProvider();
            _dispatcher = new EventDispatcher(_serviceProvider);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenServiceProviderIsNull()
        {
            Action act = () => new EventDispatcher(null);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("serviceProvider");
        }

        [Fact]
        public async Task DispatchAsync_ShouldThrowArgumentNullException_WhenEventIsNull()
        {
            Func<Task> act = async () => await _dispatcher.DispatchAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("domainEvent");
        }

        [Fact]
        public async Task DispatchAsync_ShouldCallSingleHandler()
        {
            // Arrange
            var mockHandler = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton(mockHandler.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            await dispatcher.DispatchAsync(domainEvent);

            // Assert
            mockHandler.Verify(x => x.HandleAsync(domainEvent), Times.Once);
        }

        [Fact]
        public async Task DispatchAsync_ShouldCallMultipleHandlers()
        {
            // Arrange
            var mockHandler1 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler1.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>()))
                .Returns(Task.CompletedTask);

            var mockHandler2 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler2.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton(mockHandler1.Object);
            services.AddSingleton(mockHandler2.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            await dispatcher.DispatchAsync(domainEvent);

            // Assert
            mockHandler1.Verify(x => x.HandleAsync(domainEvent), Times.Once);
            mockHandler2.Verify(x => x.HandleAsync(domainEvent), Times.Once);
        }

        [Fact]
        public async Task DispatchAsync_ShouldNotThrow_WhenNoHandlersRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            Func<Task> act = async () => await dispatcher.DispatchAsync(domainEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DispatchAsync_ShouldWaitForAllHandlers()
        {
            // Arrange
            var completionOrder = new System.Collections.Concurrent.ConcurrentBag<int>();

            var mockHandler1 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler1.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>()))
                .Returns(async () =>
                {
                    await Task.Delay(100);
                    completionOrder.Add(1);
                });

            var mockHandler2 = new Mock<IEventHandler<LogWrittenToFileEvent>>();
            mockHandler2.Setup(x => x.HandleAsync(It.IsAny<LogWrittenToFileEvent>()))
                .Returns(async () =>
                {
                    await Task.Delay(50);
                    completionOrder.Add(2);
                });

            var services = new ServiceCollection();
            services.AddSingleton(mockHandler1.Object);
            services.AddSingleton(mockHandler2.Object);
            var serviceProvider = services.BuildServiceProvider();
            var dispatcher = new EventDispatcher(serviceProvider);

            var filePath = LogFilePath.Create("test.log").Value;
            var logEntry = new LogEntry(EventType.START);
            var domainEvent = new LogWrittenToFileEvent(filePath, logEntry);

            // Act
            await dispatcher.DispatchAsync(domainEvent);

            // Assert
            completionOrder.Should().HaveCount(2);
            completionOrder.Should().Contain(new[] { 1, 2 });
        }
    }
}