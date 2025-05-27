using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using AiDevTest1.Domain.Models;
using AiDevTest1.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AiDevTest1.Tests;

/// <summary>
/// LogWriteServiceクラスのユニットテスト
/// </summary>
public class LogWriteServiceTests
{
  private readonly Mock<ILogEntryFactory> _mockLogEntryFactory;
  private readonly Mock<ILogFileHandler> _mockLogFileHandler;
  private readonly LogWriteService _service;

  public LogWriteServiceTests()
  {
    _mockLogEntryFactory = new Mock<ILogEntryFactory>();
    _mockLogFileHandler = new Mock<ILogFileHandler>();
    _service = new LogWriteService(_mockLogEntryFactory.Object, _mockLogFileHandler.Object);
  }

  /// <summary>
  /// コンストラクタでnullのlogEntryFactoryが渡された場合、ArgumentNullExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithNullLogEntryFactory_ThrowsArgumentNullException()
  {
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new LogWriteService(null!, _mockLogFileHandler.Object));

    exception.ParamName.Should().Be("logEntryFactory");
  }

  /// <summary>
  /// コンストラクタでnullのlogFileHandlerが渡された場合、ArgumentNullExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithNullLogFileHandler_ThrowsArgumentNullException()
  {
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new LogWriteService(_mockLogEntryFactory.Object, null!));

    exception.ParamName.Should().Be("logFileHandler");
  }

  /// <summary>
  /// 正常系：ログエントリの生成とファイル書き込みが成功することをテスト
  /// </summary>
  [Fact]
  public async Task WriteLogEntryAsync_WithValidDependencies_ReturnsSuccess()
  {
    // Arrange
    var expectedLogEntry = new LogEntry(EventType.START, "Test message");
    _mockLogEntryFactory
        .Setup(x => x.CreateLogEntry())
        .Returns(expectedLogEntry);

    _mockLogFileHandler
        .Setup(x => x.AppendLogEntryAsync(expectedLogEntry))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _service.WriteLogEntryAsync();

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.IsFailure.Should().BeFalse();
    result.ErrorMessage.Should().BeNull();

    // 依存関係の呼び出しを検証
    _mockLogEntryFactory.Verify(x => x.CreateLogEntry(), Times.Once);
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(expectedLogEntry), Times.Once);
  }

  /// <summary>
  /// LogEntryFactoryでArgumentExceptionが発生した場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task WriteLogEntryAsync_WhenLogEntryFactoryThrowsArgumentException_ReturnsFailure()
  {
    // Arrange
    var exceptionMessage = "Invalid EventType";
    _mockLogEntryFactory
        .Setup(x => x.CreateLogEntry())
        .Throws(new ArgumentException(exceptionMessage));

    // Act
    var result = await _service.WriteLogEntryAsync();

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().NotBeNull();
    result.ErrorMessage.Should().Contain("ログの書き込みに失敗しました");
    result.ErrorMessage.Should().Contain(exceptionMessage);

    // LogEntryFactoryは呼び出されるが、LogFileHandlerは呼び出されない
    _mockLogEntryFactory.Verify(x => x.CreateLogEntry(), Times.Once);
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(It.IsAny<LogEntry>()), Times.Never);
  }

  /// <summary>
  /// LogEntryFactoryで一般的なExceptionが発生した場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task WriteLogEntryAsync_WhenLogEntryFactoryThrowsException_ReturnsFailure()
  {
    // Arrange
    var exceptionMessage = "General factory error";
    _mockLogEntryFactory
        .Setup(x => x.CreateLogEntry())
        .Throws(new Exception(exceptionMessage));

    // Act
    var result = await _service.WriteLogEntryAsync();

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().NotBeNull();
    result.ErrorMessage.Should().Contain("ログの書き込みに失敗しました");
    result.ErrorMessage.Should().Contain(exceptionMessage);

    _mockLogEntryFactory.Verify(x => x.CreateLogEntry(), Times.Once);
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(It.IsAny<LogEntry>()), Times.Never);
  }

  /// <summary>
  /// LogFileHandlerでIOExceptionが発生した場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task WriteLogEntryAsync_WhenLogFileHandlerThrowsIOException_ReturnsFailure()
  {
    // Arrange
    var expectedLogEntry = new LogEntry(EventType.ERROR, "Test error message");
    var exceptionMessage = "Disk full";

    _mockLogEntryFactory
        .Setup(x => x.CreateLogEntry())
        .Returns(expectedLogEntry);

    _mockLogFileHandler
        .Setup(x => x.AppendLogEntryAsync(expectedLogEntry))
        .ThrowsAsync(new System.IO.IOException(exceptionMessage));

    // Act
    var result = await _service.WriteLogEntryAsync();

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().NotBeNull();
    result.ErrorMessage.Should().Contain("ログの書き込みに失敗しました");
    result.ErrorMessage.Should().Contain(exceptionMessage);

    _mockLogEntryFactory.Verify(x => x.CreateLogEntry(), Times.Once);
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(expectedLogEntry), Times.Once);
  }

  /// <summary>
  /// LogFileHandlerでUnauthorizedAccessExceptionが発生した場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task WriteLogEntryAsync_WhenLogFileHandlerThrowsUnauthorizedAccessException_ReturnsFailure()
  {
    // Arrange
    var expectedLogEntry = new LogEntry(EventType.WARN, "Test warning message");
    var exceptionMessage = "Access to the path is denied";

    _mockLogEntryFactory
        .Setup(x => x.CreateLogEntry())
        .Returns(expectedLogEntry);

    _mockLogFileHandler
        .Setup(x => x.AppendLogEntryAsync(expectedLogEntry))
        .ThrowsAsync(new UnauthorizedAccessException(exceptionMessage));

    // Act
    var result = await _service.WriteLogEntryAsync();

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().NotBeNull();
    result.ErrorMessage.Should().Contain("ログの書き込みに失敗しました");
    result.ErrorMessage.Should().Contain(exceptionMessage);

    _mockLogEntryFactory.Verify(x => x.CreateLogEntry(), Times.Once);
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(expectedLogEntry), Times.Once);
  }

  /// <summary>
  /// LogFileHandlerでTask.Delayを使った非同期例外が発生した場合のエラーハンドリングをテスト
  /// </summary>
  [Fact]
  public async Task WriteLogEntryAsync_WhenLogFileHandlerAsyncOperationFails_ReturnsFailure()
  {
    // Arrange
    var expectedLogEntry = new LogEntry(EventType.STOP, "Test stop message");
    var exceptionMessage = "Async operation failed";

    _mockLogEntryFactory
        .Setup(x => x.CreateLogEntry())
        .Returns(expectedLogEntry);

    _mockLogFileHandler
        .Setup(x => x.AppendLogEntryAsync(expectedLogEntry))
        .Returns(async () =>
        {
          await Task.Delay(10); // Simulate async work
          throw new Exception(exceptionMessage);
        });

    // Act
    var result = await _service.WriteLogEntryAsync();

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().NotBeNull();
    result.ErrorMessage.Should().Contain("ログの書き込みに失敗しました");
    result.ErrorMessage.Should().Contain(exceptionMessage);

    _mockLogEntryFactory.Verify(x => x.CreateLogEntry(), Times.Once);
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(expectedLogEntry), Times.Once);
  }

  /// <summary>
  /// 複数回連続でWriteLogEntryAsyncを呼び出した場合の動作をテスト
  /// </summary>
  [Fact]
  public async Task WriteLogEntryAsync_MultipleCalls_EachCallCreatesNewLogEntryAndWritesToFile()
  {
    // Arrange
    var logEntry1 = new LogEntry(EventType.START, "First message");
    var logEntry2 = new LogEntry(EventType.STOP, "Second message");
    var logEntry3 = new LogEntry(EventType.WARN, "Third message");

    _mockLogEntryFactory
        .SetupSequence(x => x.CreateLogEntry())
        .Returns(logEntry1)
        .Returns(logEntry2)
        .Returns(logEntry3);

    _mockLogFileHandler
        .Setup(x => x.AppendLogEntryAsync(It.IsAny<LogEntry>()))
        .Returns(Task.CompletedTask);

    // Act
    var result1 = await _service.WriteLogEntryAsync();
    var result2 = await _service.WriteLogEntryAsync();
    var result3 = await _service.WriteLogEntryAsync();

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result2.IsSuccess.Should().BeTrue();
    result3.IsSuccess.Should().BeTrue();

    _mockLogEntryFactory.Verify(x => x.CreateLogEntry(), Times.Exactly(3));
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(logEntry1), Times.Once);
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(logEntry2), Times.Once);
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(logEntry3), Times.Once);
  }

  /// <summary>
  /// 成功と失敗が混在する連続呼び出しの動作をテスト
  /// </summary>
  [Fact]
  public async Task WriteLogEntryAsync_MixedSuccessAndFailureCalls_ReturnsAppropriateResults()
  {
    // Arrange
    var logEntry1 = new LogEntry(EventType.START, "Success message");
    var logEntry2 = new LogEntry(EventType.ERROR, "Will fail message");

    _mockLogEntryFactory
        .SetupSequence(x => x.CreateLogEntry())
        .Returns(logEntry1)
        .Returns(logEntry2);

    _mockLogFileHandler
        .SetupSequence(x => x.AppendLogEntryAsync(It.IsAny<LogEntry>()))
        .Returns(Task.CompletedTask)
        .ThrowsAsync(new Exception("File write failed"));

    // Act
    var result1 = await _service.WriteLogEntryAsync();
    var result2 = await _service.WriteLogEntryAsync();

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result1.ErrorMessage.Should().BeNull();

    result2.IsSuccess.Should().BeFalse();
    result2.ErrorMessage.Should().Contain("ログの書き込みに失敗しました");
    result2.ErrorMessage.Should().Contain("File write failed");

    _mockLogEntryFactory.Verify(x => x.CreateLogEntry(), Times.Exactly(2));
    _mockLogFileHandler.Verify(x => x.AppendLogEntryAsync(It.IsAny<LogEntry>()), Times.Exactly(2));
  }
}
