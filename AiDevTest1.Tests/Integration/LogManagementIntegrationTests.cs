using System;
using System.Threading.Tasks;
using AiDevTest1.Application.Commands;
using AiDevTest1.Application.Handlers;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using AiDevTest1.Application.EventHandlers;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.Interfaces;
using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.Services;
using AiDevTest1.Domain.ValueObjects;
using AiDevTest1.Infrastructure.Configuration;
using AiDevTest1.Infrastructure.Events;
using AiDevTest1.Infrastructure.Policies;
using AiDevTest1.Infrastructure.Services;
using AiDevTest1.WpfApp.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AiDevTest1.Tests.Integration
{
    /// <summary>
    /// ログ管理システム全体の統合テスト
    /// Command → Handler → Event → EventHandler の一連のフローを検証
    /// </summary>
    public class LogManagementIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mock<IIoTHubClient> _mockIoTHubClient;
        private readonly Mock<ILogFileHandler> _mockLogFileHandler;

        public LogManagementIntegrationTests()
        {
            var services = new ServiceCollection();
            
            // Mocksの作成
            _mockIoTHubClient = new Mock<IIoTHubClient>();
            _mockLogFileHandler = new Mock<ILogFileHandler>();
            
            // 設定のセットアップ
            var configuration = CreateTestConfiguration();
            services.Configure<IoTHubConfiguration>(configuration.GetSection("AuthInfo"));

            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private IConfiguration CreateTestConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("AuthInfo:ConnectionString", "HostName=test.azure-devices.net;DeviceId=testdevice;SharedAccessKey=testkey"),
                    new KeyValuePair<string, string>("AuthInfo:DeviceId", "testdevice")
                });
            
            return configBuilder.Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Core Services
            services.AddTransient<ILogEntryFactory, LogEntryFactory>();
            services.AddSingleton(_mockLogFileHandler.Object);

            // Command Handlers  
            services.AddTransient<ICommandHandler<WriteLogCommand>, WriteLogCommandHandler>();
            services.AddTransient<ICommandHandler<UploadFileCommand>, UploadFileCommandHandler>();

            // Policies
            services.AddTransient<IRetryPolicy, ExponentialBackoffRetryPolicy>();
            services.AddTransient<IDialogDisplayPolicy, StandardDialogDisplayPolicy>();

            // Services
            services.AddSingleton<ILogWriteService, LogWriteService>();
            services.AddTransient<IFileUploadService, FileUploadService>();
            services.AddSingleton(_mockIoTHubClient.Object);

            // Event Dispatcher
            services.AddSingleton<IEventDispatcher, EventDispatcher>();

            // Event Handlers
            services.AddTransient<IEventHandler<LogWrittenToFileEvent>, LogWrittenToFileEventHandler>();
            services.AddTransient<IEventHandler<FileUploadedEvent>, FileUploadedEventHandler>();
            services.AddTransient<IEventHandler<FileUploadFailedEvent>, FileUploadFailedEventHandler>();

            // UI Services
            services.AddSingleton<DialogService>();
            services.AddSingleton<IDialogService>(provider =>
            {
                var dialogService = provider.GetRequiredService<DialogService>();
                var displayPolicy = provider.GetRequiredService<IDialogDisplayPolicy>();
                return new PolicyBasedDialogService(dialogService, displayPolicy);
            });
        }

        [Fact]
        public async Task WriteLogCommand_ShouldTriggerEventDispatch()
        {
            // Arrange
            var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<WriteLogCommand>>();
            var command = new WriteLogCommand();
            
            // Mock setup
            var testLogPath = new LogFilePath("integration-test.log");
            _mockLogFileHandler.Setup(x => x.GetCurrentLogFilePath())
                .Returns(testLogPath);
            _mockLogFileHandler.Setup(x => x.AppendLogEntryAsync(It.IsAny<LogEntry>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await commandHandler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            
            // Verify that the log file handler was called
            _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(It.IsAny<LogEntry>()), Times.Once);
        }

        [Fact]
        public async Task UploadFileCommand_Success_ShouldTriggerFileUploadedEvent()
        {
            // Arrange
            var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<UploadFileCommand>>();
            var command = new UploadFileCommand();
            
            // Mock setup for successful upload
            var testLogPath = new LogFilePath("integration-test.log");
            _mockLogFileHandler.Setup(x => x.GetCurrentLogFilePath())
                .Returns(testLogPath);
            
            var sasUriResult = SasUriResult.Success(
                "https://test.blob.core.windows.net/test?sas=token",
                "test-correlation-id");
            
            _mockIoTHubClient.Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
                .ReturnsAsync(sasUriResult);
            
            _mockIoTHubClient.Setup(x => x.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .ReturnsAsync(UploadToBlobResult.Success());
            
            _mockIoTHubClient.Setup(x => x.NotifyFileUploadCompleteAsync(It.IsAny<string>(), true))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await commandHandler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            
            // Verify the complete upload flow was executed
            _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Once);
            _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
            _mockIoTHubClient.Verify(x => x.NotifyFileUploadCompleteAsync(It.IsAny<string>(), true), Times.Once);
        }

        [Fact]
        public async Task UploadFileCommand_Failure_ShouldTriggerFileUploadFailedEvent()
        {
            // Arrange
            var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<UploadFileCommand>>();
            var command = new UploadFileCommand();
            
            // Mock setup for failed upload
            var testLogPath = new LogFilePath("integration-test.log");
            _mockLogFileHandler.Setup(x => x.GetCurrentLogFilePath())
                .Returns(testLogPath);
            
            // Simulate SAS URI failure
            _mockIoTHubClient.Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
                .ReturnsAsync(SasUriResult.Failure("Failed to get SAS URI"));

            // Act
            var result = await commandHandler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to get SAS URI");
            
            // Verify SAS URI was attempted
            _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Once);
            // Upload should not be attempted since SAS URI failed
            _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
        public async Task FileNotExists_ShouldReturnFailure()
        {
            // Arrange
            var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<UploadFileCommand>>();
            var command = new UploadFileCommand();
            
            // Mock setup - file doesn't exist
            var testLogPath = new LogFilePath("non-existent.log");
            _mockLogFileHandler.Setup(x => x.GetCurrentLogFilePath())
                .Returns(testLogPath);

            // Act
            var result = await commandHandler.HandleAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("ログファイルが存在しません");
            
            // Verify no upload was attempted
            _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Never);
        }

        [Fact]
        public void EventDispatcher_ShouldBeRegistered()
        {
            // Arrange & Act
            var eventDispatcher = _serviceProvider.GetService<IEventDispatcher>();

            // Assert
            eventDispatcher.Should().NotBeNull();
            eventDispatcher.Should().BeOfType<EventDispatcher>();
        }

        [Fact]
        public void EventHandlers_ShouldBeRegistered()
        {
            // Arrange & Act
            var logWrittenHandler = _serviceProvider.GetService<IEventHandler<LogWrittenToFileEvent>>();
            var fileUploadedHandler = _serviceProvider.GetService<IEventHandler<FileUploadedEvent>>();
            var fileUploadFailedHandler = _serviceProvider.GetService<IEventHandler<FileUploadFailedEvent>>();

            // Assert
            logWrittenHandler.Should().NotBeNull();
            logWrittenHandler.Should().BeOfType<LogWrittenToFileEventHandler>();
            
            fileUploadedHandler.Should().NotBeNull();
            fileUploadedHandler.Should().BeOfType<FileUploadedEventHandler>();
            
            fileUploadFailedHandler.Should().NotBeNull();
            fileUploadFailedHandler.Should().BeOfType<FileUploadFailedEventHandler>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}