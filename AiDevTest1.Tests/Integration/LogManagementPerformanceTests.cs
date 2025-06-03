using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Moq;
using Xunit;

namespace AiDevTest1.Tests.Integration
{
    /// <summary>
    /// ログ管理システムのパフォーマンステスト
    /// 大量データ、同時実行、レスポンス時間の検証
    /// </summary>
    public class LogManagementPerformanceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mock<IIoTHubClient> _mockIoTHubClient;
        private readonly Mock<ILogFileHandler> _mockLogFileHandler;

        public LogManagementPerformanceTests()
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

            // 高速レスポンス用のモックセットアップ
            SetupHighPerformanceMocks();
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

        private void SetupHighPerformanceMocks()
        {
            // ログファイルハンドラーの高速セットアップ
            var testLogPath = new LogFilePath("performance-test.log");
            _mockLogFileHandler.Setup(x => x.GetCurrentLogFilePath())
                .Returns(testLogPath);
            _mockLogFileHandler.Setup(x => x.AppendLogEntryAsync(It.IsAny<LogEntry>()))
                .Returns(Task.CompletedTask);

            // IoTHubクライアントの高速セットアップ
            var sasUriResult = SasUriResult.Success(
                "https://test.blob.core.windows.net/test?sas=token",
                "performance-test-correlation-id");
            
            _mockIoTHubClient.Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
                .ReturnsAsync(sasUriResult);
            
            _mockIoTHubClient.Setup(x => x.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .ReturnsAsync(UploadToBlobResult.Success());
            
            _mockIoTHubClient.Setup(x => x.NotifyFileUploadCompleteAsync(It.IsAny<string>(), true))
                .ReturnsAsync(Result.Success());
        }

        [Fact]
        public async Task WriteLogCommand_BulkExecution_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            const int commandCount = 100;
            const int maxExecutionTimeMs = 5000; // 5秒以内に100件処理

            var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<WriteLogCommand>>();
            var commands = new List<WriteLogCommand>();
            
            for (int i = 0; i < commandCount; i++)
            {
                commands.Add(new WriteLogCommand());
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task<Result>>();
            
            foreach (var command in commands)
            {
                tasks.Add(commandHandler.HandleAsync(command));
            }
            
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxExecutionTimeMs,
                $"100件のログコマンドを{maxExecutionTimeMs}ms以内に処理する必要があります");
            
            results.Should().AllSatisfy(result => 
            {
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
            });

            // パフォーマンスメトリクスの出力
            var averageTimePerCommand = stopwatch.ElapsedMilliseconds / (double)commandCount;
            Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average time per command: {averageTimePerCommand:F2}ms");
            Console.WriteLine($"Commands per second: {commandCount / (stopwatch.ElapsedMilliseconds / 1000.0):F2}");
        }

        [Fact]
        public async Task UploadFileCommand_ConcurrentExecution_ShouldHandleParallelRequests()
        {
            // Arrange
            const int concurrentRequests = 10;
            const int maxExecutionTimeMs = 3000; // 3秒以内

            var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<UploadFileCommand>>();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task<Result>>();
            
            for (int i = 0; i < concurrentRequests; i++)
            {
                tasks.Add(commandHandler.HandleAsync(new UploadFileCommand()));
            }
            
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxExecutionTimeMs,
                $"{concurrentRequests}件の並行アップロードを{maxExecutionTimeMs}ms以内に処理する必要があります");
            
            results.Should().AllSatisfy(result => 
            {
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
            });

            // 並行処理の検証
            _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), 
                Times.Exactly(concurrentRequests));
            
            Console.WriteLine($"Concurrent uploads completion time: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average time per upload: {stopwatch.ElapsedMilliseconds / (double)concurrentRequests:F2}ms");
        }

        [Fact]
        public async Task EventDispatcher_HighVolumeEvents_ShouldMaintainPerformance()
        {
            // Arrange
            const int eventCount = 200;
            const int maxExecutionTimeMs = 2000; // 2秒以内

            var eventDispatcher = _serviceProvider.GetRequiredService<IEventDispatcher>();
            var events = new List<LogWrittenToFileEvent>();
            
            for (int i = 0; i < eventCount; i++)
            {
                var logEntry = new LogEntry(EventType.START, $"Performance test message {i}");
                var logPath = new LogFilePath($"performance-test-{i}.log");
                events.Add(new LogWrittenToFileEvent(logPath, logEntry));
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            
            foreach (var eventItem in events)
            {
                tasks.Add(eventDispatcher.DispatchAsync(eventItem));
            }
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxExecutionTimeMs,
                $"{eventCount}件のイベントディスパッチを{maxExecutionTimeMs}ms以内に処理する必要があります");
            
            Console.WriteLine($"Event dispatch completion time: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Events per second: {eventCount / (stopwatch.ElapsedMilliseconds / 1000.0):F2}");
        }

        [Fact]
        public async Task CommandHandler_MemoryUsage_ShouldNotCauseMemoryLeak()
        {
            // Arrange
            const int iterationCount = 50;
            const long maxMemoryIncreaseMB = 10; // 10MB以下の増加量

            var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<WriteLogCommand>>();
            
            // 初期メモリ使用量の測定
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);

            // Act
            for (int i = 0; i < iterationCount; i++)
            {
                var command = new WriteLogCommand();
                var result = await commandHandler.HandleAsync(command);
                result.IsSuccess.Should().BeTrue();
                
                // 定期的にGCを実行してメモリを解放
                if (i % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }

            // 最終メモリ使用量の測定
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncreaseMB = (finalMemory - initialMemory) / (1024.0 * 1024.0);

            // Assert
            memoryIncreaseMB.Should().BeLessThan(maxMemoryIncreaseMB,
                $"メモリ使用量の増加は{maxMemoryIncreaseMB}MB以下である必要があります");
            
            Console.WriteLine($"Initial memory: {initialMemory / (1024.0 * 1024.0):F2}MB");
            Console.WriteLine($"Final memory: {finalMemory / (1024.0 * 1024.0):F2}MB");
            Console.WriteLine($"Memory increase: {memoryIncreaseMB:F2}MB");
        }

        [Fact]
        public void ServiceResolution_ShouldBeEfficient()
        {
            // Arrange
            const int resolutionCount = 1000;
            const int maxExecutionTimeMs = 100; // 100ms以内

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < resolutionCount; i++)
            {
                var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<WriteLogCommand>>();
                var eventDispatcher = _serviceProvider.GetRequiredService<IEventDispatcher>();
                var logWriteService = _serviceProvider.GetRequiredService<ILogWriteService>();
                
                // 解決されたサービスが有効であることを確認
                commandHandler.Should().NotBeNull();
                eventDispatcher.Should().NotBeNull();
                logWriteService.Should().NotBeNull();
            }
            
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxExecutionTimeMs,
                $"{resolutionCount}回のサービス解決を{maxExecutionTimeMs}ms以内に完了する必要があります");
            
            Console.WriteLine($"Service resolution time: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Resolutions per second: {resolutionCount / (stopwatch.ElapsedMilliseconds / 1000.0):F2}");
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}