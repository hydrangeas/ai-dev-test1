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
/// FileUploadServiceクラスのユニットテスト
/// </summary>
public class FileUploadServiceTests
{
  private readonly Mock<IIoTHubClient> _mockIoTHubClient;
  private readonly Mock<ILogFileHandler> _mockLogFileHandler;
  private readonly FileUploadService _service;

  public FileUploadServiceTests()
  {
    _mockIoTHubClient = new Mock<IIoTHubClient>();
    _mockLogFileHandler = new Mock<ILogFileHandler>();
    _service = new FileUploadService(_mockIoTHubClient.Object, _mockLogFileHandler.Object);
  }

  /// <summary>
  /// コンストラクタでnullのioTHubClientが渡された場合、ArgumentNullExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithNullIoTHubClient_ThrowsArgumentNullException()
  {
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new FileUploadService(null!, _mockLogFileHandler.Object));

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
        new FileUploadService(_mockIoTHubClient.Object, null!));

    exception.ParamName.Should().Be("logFileHandler");
  }

  /// <summary>
  /// 正常系：完全なアップロードワークフローが成功することをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WithValidWorkflow_ReturnsSuccess()
  {
    // Arrange
    // 実際のファイルを作成してテスト
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
      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(new BlobName("2023-12-25.log")), Times.Once);
      _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()), Times.Once);
      _mockIoTHubClient.Verify(x => x.NotifyFileUploadCompleteAsync(correlationId, true), Times.Once);
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
    _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Never);
  }

  /// <summary>
  /// SAS URI取得が全ての試行で失敗した場合のリトライロジックをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenSasUriFailsAllRetries_ReturnsFailureWithAllAttempts()
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

      _mockIoTHubClient
          .Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
          .ReturnsAsync(SasUriResult.Failure("SAS URI generation failed"));

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeFalse();
      result.IsFailure.Should().BeTrue();
      result.ErrorMessage.Should().NotBeNull();
      result.ErrorMessage.Should().Contain("ファイルアップロードが3回の試行すべてで失敗しました");
      result.ErrorMessage.Should().Contain("試行1: SAS URI取得に失敗");
      result.ErrorMessage.Should().Contain("試行2: SAS URI取得に失敗");
      result.ErrorMessage.Should().Contain("試行3: SAS URI取得に失敗");
      result.ErrorMessage.Should().Contain("SAS URI generation failed");

      // 3回のリトライが実行されたことを確認
      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Exactly(3));
      _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
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

  /// <summary>
  /// SAS URIが空文字列の場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenSasUriIsEmpty_ReturnsFailureAfterRetries()
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

      _mockIoTHubClient
          .Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
          .ReturnsAsync(SasUriResult.Success("", "correlation-id")); // 空のSAS URI

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeFalse();
      result.IsFailure.Should().BeTrue();
      result.ErrorMessage.Should().NotBeNull();
      result.ErrorMessage.Should().Contain("ファイルアップロードが3回の試行すべてで失敗しました");
      result.ErrorMessage.Should().Contain("試行1: SAS URIが空");
      result.ErrorMessage.Should().Contain("試行2: SAS URIが空");
      result.ErrorMessage.Should().Contain("試行3: SAS URIが空");

      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Exactly(3));
      _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
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
  /// Blobアップロードが全ての試行で失敗した場合のリトライロジックをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenBlobUploadFailsAllRetries_ReturnsFailureWithAllAttempts()
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

      _mockIoTHubClient
          .Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
          .ReturnsAsync(SasUriResult.Success(sasUri, correlationId));

      _mockIoTHubClient
          .Setup(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()))
          .ReturnsAsync(UploadToBlobResult.Failure("Blob upload failed"));

      _mockIoTHubClient
          .Setup(x => x.NotifyFileUploadCompleteAsync(correlationId, false))
          .ReturnsAsync(Result.Success());

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeFalse();
      result.IsFailure.Should().BeTrue();
      result.ErrorMessage.Should().NotBeNull();
      result.ErrorMessage.Should().Contain("ファイルアップロードが3回の試行すべてで失敗しました");
      result.ErrorMessage.Should().Contain("試行1: Blobアップロードに失敗");
      result.ErrorMessage.Should().Contain("試行2: Blobアップロードに失敗");
      result.ErrorMessage.Should().Contain("試行3: Blobアップロードに失敗");
      result.ErrorMessage.Should().Contain("Blob upload failed");

      // 3回のリトライが実行されたことを確認
      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Exactly(3));
      _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()), Times.Exactly(3));
      // 最後の試行で失敗通知が送信されることを確認
      _mockIoTHubClient.Verify(x => x.NotifyFileUploadCompleteAsync(correlationId, false), Times.Once);
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
  /// 最初の試行で成功した場合、リトライが実行されないことをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenFirstAttemptSucceeds_DoesNotRetry()
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

      _mockIoTHubClient
          .Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
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
      result.ErrorMessage.Should().BeNull();

      // 1回のみ実行されたことを確認（リトライなし）
      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Once);
      _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()), Times.Once);
      _mockIoTHubClient.Verify(x => x.NotifyFileUploadCompleteAsync(correlationId, true), Times.Once);
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
  /// 2回目の試行で成功した場合のリトライロジックをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenSecondAttemptSucceeds_RetriesOnceAndSucceeds()
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

      _mockIoTHubClient
          .SetupSequence(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
          .ReturnsAsync(SasUriResult.Failure("First attempt failed"))
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
      result.ErrorMessage.Should().BeNull();

      // 2回実行されたことを確認
      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Exactly(2));
      _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()), Times.Once);
      _mockIoTHubClient.Verify(x => x.NotifyFileUploadCompleteAsync(correlationId, true), Times.Once);
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

      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Once);
      _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()), Times.Once);
      _mockIoTHubClient.Verify(x => x.NotifyFileUploadCompleteAsync(correlationId, true), Times.Once);
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

      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Once);
      _mockIoTHubClient.Verify(x => x.UploadToBlobAsync(sasUri, It.IsAny<byte[]>()), Times.Once);
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

  /// <summary>
  /// 予期しない例外が発生した場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenUnexpectedExceptionOccurs_ReturnsFailure()
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

      _mockIoTHubClient
          .Setup(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()))
          .ThrowsAsync(new InvalidOperationException("Unexpected error"));

      // Act
      var result = await _service.UploadLogFileAsync();

      // Assert
      result.Should().NotBeNull();
      result.IsSuccess.Should().BeFalse();
      result.IsFailure.Should().BeTrue();
      result.ErrorMessage.Should().NotBeNull();
      result.ErrorMessage.Should().Contain("ファイルアップロードが3回の試行すべてで失敗しました");
      result.ErrorMessage.Should().Contain("試行1: 予期しないエラー - Unexpected error");
      result.ErrorMessage.Should().Contain("試行2: 予期しないエラー - Unexpected error");
      result.ErrorMessage.Should().Contain("試行3: 予期しないエラー - Unexpected error");

      _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Exactly(3));
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
  /// ファイル読み取りで例外が発生した場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task UploadLogFileAsync_WhenFileReadFails_ReturnsFailure()
  {
    // Arrange
    var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    var invalidFilePath = Path.Combine(tempDirectory, "invalid-path", "test.log");
    // 存在しないパスをシミュレート
    var logFilePath = new LogFilePath(invalidFilePath);

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

    _mockLogFileHandler.Verify(x => x.GetCurrentLogFilePath(), Times.Once);
    _mockIoTHubClient.Verify(x => x.GetFileUploadSasUriAsync(It.IsAny<BlobName>()), Times.Never);
  }
}
