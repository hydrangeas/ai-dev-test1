using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using AiDevTest1.Domain.ValueObjects;
using AiDevTest1.Infrastructure.Services;
using FluentAssertions;
using Moq;
using System.IO;
using Xunit;

namespace AiDevTest1.Tests;

/// <summary>
/// FileUploadServiceクラスのユニットテスト（RetryPolicy使用版）
/// </summary>
public class FileUploadServiceTests
{
  private readonly Mock<IIoTHubClient> _mockIoTHubClient;
  private readonly Mock<ILogFileHandler> _mockLogFileHandler;
  private readonly Mock<IRetryPolicy> _mockRetryPolicy;
  private readonly FileUploadService _service;

  public FileUploadServiceTests()
  {
    _mockIoTHubClient = new Mock<IIoTHubClient>();
    _mockLogFileHandler = new Mock<ILogFileHandler>();
    _mockRetryPolicy = new Mock<IRetryPolicy>();
    _service = new FileUploadService(_mockIoTHubClient.Object, _mockLogFileHandler.Object, _mockRetryPolicy.Object);
  }

  /// <summary>
  /// コンストラクタでnullのioTHubClientが渡された場合、ArgumentNullExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithNullIoTHubClient_ThrowsArgumentNullException()
  {
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new FileUploadService(null!, _mockLogFileHandler.Object, _mockRetryPolicy.Object));

    exception.ParamName.Should().Be("ioTHubClient");
  }

  /// <summary>
  /// コンストラクタでnullのlogFileHandlerが渡された場合、ArgumentNullExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithNullLogFileHandler_ThrowsArgumentNullException()
  {
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new FileUploadService(_mockIoTHubClient.Object, null!, _mockRetryPolicy.Object));

    exception.ParamName.Should().Be("logFileHandler");
  }

  /// <summary>
  /// コンストラクタでnullのretryPolicyが渡された場合、ArgumentNullExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithNullRetryPolicy_ThrowsArgumentNullException()
  {
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new FileUploadService(_mockIoTHubClient.Object, _mockLogFileHandler.Object, null!));

    exception.ParamName.Should().Be("retryPolicy");
  }

  /// <summary>
  /// 正常系：完全なアップロードワークフローが成功することをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WithValidWorkflow_ReturnsSuccess()
  {
    // Arrange
    var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDirectory);
    var tempFilePath = Path.Combine(tempDirectory, "2023-12-25.log");
    var fileContent = "test log content"u8.ToArray();
    await File.WriteAllBytesAsync(tempFilePath, fileContent);

    try
    {
      var logFilePath = new LogFilePath(tempFilePath);
      var sasUri = "https://test.blob.core.windows.net/logs/2023-12-25.log?sas=token";
      var correlationId = "test-correlation-id";

      _mockLogFileHandler
          .Setup(x => x.GetCurrentLogFilePath())
          .Returns(logFilePath);

      // RetryPolicyが成功を返すようにモック
      _mockRetryPolicy
          .Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
          .Returns<Func<Task<Result>>, CancellationToken>((operation, ct) => operation());

      _mockIoTHubClient
          .Setup(x => x.GetFileUploadSasUriAsync(new BlobName("2023-12-25.log")))
          .ReturnsAsync(SasUriResult.Success(sasUri, correlationId));

      _mockIoTHubClient
          .Setup(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()))
          .ReturnsAsync(UploadToBlobResult.Success());

      _mockIoTHubClient
          .Setup(x => x.NotifyFileUploadCompleteAsync(correlationId, true))
          .ReturnsAsync(Result.Success());

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeTrue();
      result.IsFailure.Should().BeFalse();

      _mockLogFileHandler.Verify(x => x.GetCurrentLogFilePath(), Times.Once);
      _mockRetryPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    finally
    {
      // Cleanup
      if (Directory.Exists(tempDirectory))
      {
        Directory.Delete(tempDirectory, true);
      }
    }
  }

  /// <summary>
  /// ファイルが存在しない場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenFileDoesNotExist_ReturnsFailure()
  {
    // Arrange
    var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    var nonExistentFilePath = Path.Combine(tempDirectory, "non-existent-file.log");
    var logFilePath = new LogFilePath(nonExistentFilePath);

    _mockLogFileHandler
        .Setup(x => x.GetCurrentLogFilePath())
        .Returns(logFilePath);

    // Act
    var result = await _service.UploadLogFileAsync();

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().NotBeNull();
    result.ErrorMessage.Should().Contain("ログファイルが存在しません");
    result.ErrorMessage.Should().Contain(nonExistentFilePath);

    _mockLogFileHandler.Verify(x => x.GetCurrentLogFilePath(), Times.Once);
    _mockRetryPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  /// <summary>
  /// RetryPolicyがAggregateExceptionをスローした場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenRetryPolicyThrowsAggregateException_ReturnsFailureWithCombinedMessage()
  {
    // Arrange
    var tempDirectory = Path.GetTempPath();
    var tempFilePath = Path.Combine(tempDirectory, $"test-{Guid.NewGuid()}.log");
    var fileContent = "test content"u8.ToArray();
    await File.WriteAllBytesAsync(tempFilePath, fileContent);

    try
    {
      var logFilePath = new LogFilePath(tempFilePath);
      _mockLogFileHandler
          .Setup(x => x.GetCurrentLogFilePath())
          .Returns(logFilePath);

      var innerExceptions = new[]
      {
        new InvalidOperationException("First attempt failed"),
        new InvalidOperationException("Second attempt failed"),
        new InvalidOperationException("Third attempt failed")
      };
      var aggregateException = new AggregateException("Multiple failures occurred", innerExceptions);

      _mockRetryPolicy
          .Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
          .ThrowsAsync(aggregateException);

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeFalse();
      result.IsFailure.Should().BeTrue();
      result.ErrorMessage.Should().NotBeNull();
      result.ErrorMessage.Should().Contain("ファイルアップロードが全ての試行で失敗しました");
      result.ErrorMessage.Should().Contain("First attempt failed");
      result.ErrorMessage.Should().Contain("Second attempt failed");
      result.ErrorMessage.Should().Contain("Third attempt failed");

      _mockRetryPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    finally
    {
      // Cleanup
      if (File.Exists(tempFilePath))
      {
        File.Delete(tempFilePath);
      }
    }
  }

  /// <summary>
  /// RetryPolicyが一般的な例外をスローした場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenRetryPolicyThrowsGeneralException_ReturnsFailure()
  {
    // Arrange
    var tempDirectory = Path.GetTempPath();
    var tempFilePath = Path.Combine(tempDirectory, $"test-{Guid.NewGuid()}.log");
    var fileContent = "test content"u8.ToArray();
    await File.WriteAllBytesAsync(tempFilePath, fileContent);

    try
    {
      var logFilePath = new LogFilePath(tempFilePath);
      _mockLogFileHandler
          .Setup(x => x.GetCurrentLogFilePath())
          .Returns(logFilePath);

      var expectedException = new InvalidOperationException("Unexpected error from retry policy");

      _mockRetryPolicy
          .Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
          .ThrowsAsync(expectedException);

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeFalse();
      result.IsFailure.Should().BeTrue();
      result.ErrorMessage.Should().NotBeNull();
      result.ErrorMessage.Should().Contain("ファイルアップロード処理で予期しないエラーが発生しました");
      result.ErrorMessage.Should().Contain("Unexpected error from retry policy");

      _mockRetryPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    finally
    {
      // Cleanup
      if (File.Exists(tempFilePath))
      {
        File.Delete(tempFilePath);
      }
    }
  }

  /// <summary>
  /// 完了通知が失敗した場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenNotificationFails_ReturnsFailure()
  {
    // Arrange
    var tempDirectory = Path.GetTempPath();
    var tempFilePath = Path.Combine(tempDirectory, $"test-{Guid.NewGuid()}.log");
    var fileContent = "test content"u8.ToArray();
    await File.WriteAllBytesAsync(tempFilePath, fileContent);

    try
    {
      var sasUri = "https://test.blob.core.windows.net/logs/test.log?sas=token";
      var correlationId = "test-correlation-id";

      var logFilePath = new LogFilePath(tempFilePath);
      _mockLogFileHandler
          .Setup(x => x.GetCurrentLogFilePath())
          .Returns(logFilePath);

      // RetryPolicyが実際の操作を実行するようにモック
      _mockRetryPolicy
          .Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
          .Returns<Func<Task<Result>>, CancellationToken>((operation, ct) => operation());

      _mockIoTHubClient
          .Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
          .ReturnsAsync(SasUriResult.Success(sasUri, correlationId));

      _mockIoTHubClient
          .Setup(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()))
          .ReturnsAsync(UploadToBlobResult.Success());

      _mockIoTHubClient
          .Setup(x => x.NotifyFileUploadCompleteAsync(correlationId, true))
          .ReturnsAsync(Result.Failure("Notification failed"));

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeFalse();
      result.IsFailure.Should().BeTrue();
      result.ErrorMessage.Should().NotBeNull();
      result.ErrorMessage.Should().Contain("アップロード完了通知に失敗しました");
      result.ErrorMessage.Should().Contain("Notification failed");
    }
    finally
    {
      // Cleanup
      if (File.Exists(tempFilePath))
      {
        File.Delete(tempFilePath);
      }
    }
  }

  /// <summary>
  /// correlationIdが空の場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenCorrelationIdIsEmpty_ReturnsFailure()
  {
    // Arrange
    var tempDirectory = Path.GetTempPath();
    var tempFilePath = Path.Combine(tempDirectory, $"test-{Guid.NewGuid()}.log");
    var fileContent = "test content"u8.ToArray();
    await File.WriteAllBytesAsync(tempFilePath, fileContent);

    try
    {
      var sasUri = "https://test.blob.core.windows.net/logs/test.log?sas=token";
      var emptyCorrelationId = "";

      var logFilePath = new LogFilePath(tempFilePath);
      _mockLogFileHandler
          .Setup(x => x.GetCurrentLogFilePath())
          .Returns(logFilePath);

      // RetryPolicyが実際の操作を実行するようにモック
      _mockRetryPolicy
          .Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
          .Returns<Func<Task<Result>>, CancellationToken>((operation, ct) => operation());

      _mockIoTHubClient
          .Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
          .ReturnsAsync(SasUriResult.Success(sasUri, emptyCorrelationId));

      _mockIoTHubClient
          .Setup(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()))
          .ReturnsAsync(UploadToBlobResult.Success());

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeFalse();
      result.IsFailure.Should().BeTrue();
      result.ErrorMessage.Should().NotBeNull();
      result.ErrorMessage.Should().Contain("相関IDが空です");

      _mockIoTHubClient.Verify(x => x.NotifyFileUploadCompleteAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }
    finally
    {
      // Cleanup
      if (File.Exists(tempFilePath))
      {
        File.Delete(tempFilePath);
      }
    }
  }
}
