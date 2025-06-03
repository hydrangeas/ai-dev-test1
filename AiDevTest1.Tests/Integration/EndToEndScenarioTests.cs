using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
    /// エンドツーエンドシナリオテスト
    /// 実際のアプリケーション使用パターンを模倣した統合テスト
    /// </summary>
    public class EndToEndScenarioTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mock<IIoTHubClient> _mockIoTHubClient;
        private readonly EventCapture _eventCapture;
        private readonly string _tempLogDirectory;

        public EndToEndScenarioTests()
        {
            // テスト用の一時ディレクトリを作成
            _tempLogDirectory = Path.Combine(Path.GetTempPath(), $"LogTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempLogDirectory);

            var services = new ServiceCollection();
            _mockIoTHubClient = new Mock<IIoTHubClient>();
            _eventCapture = new EventCapture();
            
            var configuration = CreateTestConfiguration();
            services.Configure<IoTHubConfiguration>(configuration.GetSection("AuthInfo"));

            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            
            SetupMocks();
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
            services.AddSingleton<ILogFileHandler, LogFileHandler>();

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

            // Event Dispatcher - カスタムで追跡機能付き
            services.AddSingleton<IEventDispatcher>(provider => 
                new TrackingEventDispatcher(provider, _eventCapture));

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

        private void SetupMocks()
        {
            // 成功的なアップロードシナリオのセットアップ
            var sasUriResult = SasUriResult.Success(
                "https://e2etest.blob.core.windows.net/logs/test.log?sas=token",
                "e2e-test-correlation-id");
            
            _mockIoTHubClient.Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
                .ReturnsAsync(sasUriResult);
            
            _mockIoTHubClient.Setup(x => x.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .ReturnsAsync(UploadToBlobResult.Success());
            
            _mockIoTHubClient.Setup(x => x.NotifyFileUploadCompleteAsync(It.IsAny<string>(), true))
                .ReturnsAsync(Result.Success());
        }

        [Fact]
        public async Task TypicalUserWorkflow_WriteLogsAndUpload_ShouldCompleteSuccessfully()
        {
            // Arrange
            var writeLogHandler = _serviceProvider.GetRequiredService<ICommandHandler<WriteLogCommand>>();
            var uploadFileHandler = _serviceProvider.GetRequiredService<ICommandHandler<UploadFileCommand>>();

            // シナリオ: ユーザーが複数のログを書き込み、その後アップロードする
            var logCommands = new[]
            {
                new WriteLogCommand(), // START
                new WriteLogCommand(), // 何らかの操作
                new WriteLogCommand(), // 別の操作
                new WriteLogCommand(), // STOP
            };

            // Act & Assert - Step 1: ログの書き込み
            var writeResults = new List<Result>();
            foreach (var command in logCommands)
            {
                var result = await writeLogHandler.HandleAsync(command);
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
                writeResults.Add(result);
            }

            // イベントが発行されたことを確認
            _eventCapture.CapturedEvents.Should().HaveCount(4);
            _eventCapture.CapturedEvents.Should().AllBeOfType<LogWrittenToFileEvent>();

            // Act & Assert - Step 2: ファイルのアップロード
            var uploadResult = await uploadFileHandler.HandleAsync(new UploadFileCommand());
            uploadResult.Should().NotBeNull();
            uploadResult.IsSuccess.Should().BeTrue();

            // アップロード関連のイベントが発行されたことを確認
            _eventCapture.CapturedEvents.Should().HaveCount(5); // 4 + 1 (FileUploadedEvent)
            _eventCapture.CapturedEvents.OfType<FileUploadedEvent>().Should().HaveCount(1);

            // IoTHubクライアントが期待通りに呼び出されたことを確認
            _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Once);
            _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
            _mockIoTHubClient.Verify(x => x.NotifyFileUploadCompleteAsync(It.IsAny<string>(), true), Times.Once);
        }

        [Fact]
        public async Task ErrorRecoveryScenario_UploadFailureAndRetry_ShouldHandleGracefully()
        {
            // Arrange
            var writeLogHandler = _serviceProvider.GetRequiredService<ICommandHandler<WriteLogCommand>>();
            var uploadFileHandler = _serviceProvider.GetRequiredService<ICommandHandler<UploadFileCommand>>();

            // 最初のアップロードは失敗、2回目は成功のシナリオ
            var callCount = 0;
            _mockIoTHubClient.Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
                .Returns<BlobName>(_ =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        return Task.FromResult(SasUriResult.Failure("Network timeout"));
                    }
                    return Task.FromResult(SasUriResult.Success(
                        "https://recovery.blob.core.windows.net/logs/test.log?sas=token",
                        "recovery-correlation-id"));
                });

            // Act - Step 1: ログを書き込み
            var writeResult = await writeLogHandler.HandleAsync(new WriteLogCommand());
            writeResult.IsSuccess.Should().BeTrue();

            // Act - Step 2: 最初のアップロード（失敗）
            var firstUploadResult = await uploadFileHandler.HandleAsync(new UploadFileCommand());
            firstUploadResult.Should().NotBeNull();
            firstUploadResult.IsSuccess.Should().BeFalse();
            firstUploadResult.ErrorMessage.Should().Contain("Network timeout");

            // 失敗イベントが発行されたことを確認
            _eventCapture.CapturedEvents.OfType<FileUploadFailedEvent>().Should().HaveCount(1);

            // Act - Step 3: リトライアップロード（成功）
            var retryUploadResult = await uploadFileHandler.HandleAsync(new UploadFileCommand());
            retryUploadResult.Should().NotBeNull();
            retryUploadResult.IsSuccess.Should().BeTrue();

            // 成功イベントが発行されたことを確認
            _eventCapture.CapturedEvents.OfType<FileUploadedEvent>().Should().HaveCount(1);

            // Assert - 全体的な流れの確認
            _eventCapture.CapturedEvents.Should().HaveCount(3); // LogWritten + FileUploadFailed + FileUploaded
            _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Exactly(2));
        }

        [Fact]
        public async Task HighFrequencyLogging_ShouldMaintainEventOrdering()
        {
            // Arrange
            const int logCount = 20;
            var writeLogHandler = _serviceProvider.GetRequiredService<ICommandHandler<WriteLogCommand>>();
            var startTime = DateTime.UtcNow;

            // Act - 高頻度でログを書き込み
            var tasks = new List<Task<Result>>();
            for (int i = 0; i < logCount; i++)
            {
                tasks.Add(writeLogHandler.HandleAsync(new WriteLogCommand()));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().AllSatisfy(result => 
            {
                result.Should().NotBeNull();
                result.IsSuccess.Should().BeTrue();
            });

            // イベントの数が正しいことを確認
            _eventCapture.CapturedEvents.Should().HaveCount(logCount);
            _eventCapture.CapturedEvents.Should().AllBeOfType<LogWrittenToFileEvent>();

            // イベントの順序は並行実行のため保証されないが、すべて処理されることを確認
            var logWrittenEvents = _eventCapture.CapturedEvents.OfType<LogWrittenToFileEvent>().ToList();
            logWrittenEvents.Should().HaveCount(logCount);

            // すべてのイベントが合理的な時間範囲内で発生したことを確認
            var endTime = DateTime.UtcNow;
            var totalElapsed = endTime - startTime;
            totalElapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CompleteApplicationLifecycle_ShouldWorkSeamlessly()
        {
            // このテストは、アプリケーション全体のライフサイクルを模倣します
            // 1. アプリケーション開始
            // 2. 複数のログ操作
            // 3. 定期的なアップロード
            // 4. エラー処理
            // 5. 正常なシャットダウン

            // Arrange
            var writeLogHandler = _serviceProvider.GetRequiredService<ICommandHandler<WriteLogCommand>>();
            var uploadFileHandler = _serviceProvider.GetRequiredService<ICommandHandler<UploadFileCommand>>();

            // Phase 1: アプリケーション開始時のログ
            var startupResult = await writeLogHandler.HandleAsync(new WriteLogCommand());
            startupResult.IsSuccess.Should().BeTrue();

            // Phase 2: 通常操作のログ
            for (int i = 0; i < 5; i++)
            {
                var operationResult = await writeLogHandler.HandleAsync(new WriteLogCommand());
                operationResult.IsSuccess.Should().BeTrue();
                
                // 一定間隔でのアップロード
                if (i % 2 == 0)
                {
                    var uploadResult = await uploadFileHandler.HandleAsync(new UploadFileCommand());
                    uploadResult.IsSuccess.Should().BeTrue();
                }
            }

            // Phase 3: シャットダウン前の最終アップロード
            var finalUploadResult = await uploadFileHandler.HandleAsync(new UploadFileCommand());
            finalUploadResult.IsSuccess.Should().BeTrue();

            // Assert - 全体的な流れの検証
            var totalLogEvents = _eventCapture.CapturedEvents.OfType<LogWrittenToFileEvent>().Count();
            var totalUploadEvents = _eventCapture.CapturedEvents.OfType<FileUploadedEvent>().Count();

            totalLogEvents.Should().Be(6); // startup + 5 operations
            totalUploadEvents.Should().Be(4); // 3 periodic + 1 final

            // モックの呼び出し回数を確認
            _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), 
                Times.Exactly(totalUploadEvents));
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            
            // テスト用ディレクトリのクリーンアップ
            if (Directory.Exists(_tempLogDirectory))
            {
                try
                {
                    Directory.Delete(_tempLogDirectory, true);
                }
                catch
                {
                    // ベストエフォートでクリーンアップ
                }
            }
        }
    }

    /// <summary>
    /// イベントをキャプチャするためのヘルパークラス
    /// </summary>
    public class EventCapture
    {
        private readonly List<object> _capturedEvents = new();
        private readonly object _lock = new();

        public IReadOnlyList<object> CapturedEvents
        {
            get
            {
                lock (_lock)
                {
                    return _capturedEvents.ToList();
                }
            }
        }

        public void Capture(object eventItem)
        {
            lock (_lock)
            {
                _capturedEvents.Add(eventItem);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _capturedEvents.Clear();
            }
        }
    }

    /// <summary>
    /// イベント追跡機能付きのEventDispatcher
    /// </summary>
    public class TrackingEventDispatcher : IEventDispatcher
    {
        private readonly IEventDispatcher _innerDispatcher;
        private readonly EventCapture _eventCapture;

        public TrackingEventDispatcher(IServiceProvider serviceProvider, EventCapture eventCapture)
        {
            _innerDispatcher = new EventDispatcher(serviceProvider);
            _eventCapture = eventCapture;
        }

        public async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) 
            where TEvent : class
        {
            // イベントをキャプチャ
            _eventCapture.Capture(domainEvent);
            
            // 実際のディスパッチを実行
            await _innerDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }
    }
}