using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Models;
using AiDevTest1.Infrastructure.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace AiDevTest1.Tests
{
  public class LogFileHandlerTests
  {
    [Fact]
    public void GetCurrentLogFilePath_ShouldReturnValidPath()
    {
      // Arrange
      ILogFileHandler handler = new LogFileHandler();

      // Act
      var filePath = handler.GetCurrentLogFilePath();

      // Assert
      Assert.NotNull(filePath);
      Assert.NotEmpty(filePath);
      Assert.True(filePath.EndsWith(".log"));
      Assert.True(Path.IsPathFullyQualified(filePath));
    }

    [Fact]
    public void GetCurrentLogFilePath_ShouldReturnCorrectDateFormat()
    {
      // Arrange
      ILogFileHandler handler = new LogFileHandler();

      // Act
      var filePath = handler.GetCurrentLogFilePath();

      // Assert
      var fileName = Path.GetFileName(filePath);

      // ファイル名がyyyy-MM-dd.log形式であることを確認
      Assert.Matches(@"^\d{4}-\d{2}-\d{2}\.log$", fileName);

      // 日付部分が有効な日付であることを確認
      var datePart = fileName.Substring(0, 10); // yyyy-MM-dd部分を取得
      Assert.True(DateTime.TryParseExact(datePart, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _));
    }

    [Fact]
    public void GetCurrentLogFilePath_ShouldReturnPathInBaseDirectory()
    {
      // Arrange
      ILogFileHandler handler = new LogFileHandler();
      var expectedBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

      // Act
      var filePath = handler.GetCurrentLogFilePath();

      // Assert
      var actualDirectory = Path.GetDirectoryName(filePath);
      Assert.Equal(expectedBaseDirectory.TrimEnd(Path.DirectorySeparatorChar), actualDirectory);
    }

    [Fact]
    public async Task AppendLogEntryAsync_ShouldAppendLogEntryToFile()
    {
      // Arrange
      var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDirectory);
      ILogFileHandler handler = new LogFileHandler(baseDirectory: tempDirectory);
      var logEntry = new LogEntry(EventType.START, "Test login event");

      try
      {
        // Act
        await handler.AppendLogEntryAsync(logEntry);

        // Assert
        var filePath = handler.GetCurrentLogFilePath();
        Assert.True(File.Exists(filePath));

        var content = await File.ReadAllTextAsync(filePath);
        Assert.NotEmpty(content);
        Assert.Contains("Test login event", content);
        Assert.Contains("\"eventType\":\"START\"", content);
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

    [Fact]
    public async Task AppendLogEntryAsync_ShouldCreateFileIfNotExists()
    {
      // Arrange
      var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDirectory);
      ILogFileHandler handler = new LogFileHandler(baseDirectory: tempDirectory);
      var logEntry = new LogEntry(EventType.STOP, "Test logout event");

      try
      {
        var filePath = handler.GetCurrentLogFilePath();
        Assert.False(File.Exists(filePath)); // ファイルが存在しないことを確認

        // Act
        await handler.AppendLogEntryAsync(logEntry);

        // Assert
        Assert.True(File.Exists(filePath)); // ファイルが作成されたことを確認

        var content = await File.ReadAllTextAsync(filePath);
        Assert.NotEmpty(content);
        Assert.Contains("Test logout event", content);
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

    [Fact]
    public async Task AppendLogEntryAsync_ShouldThrowArgumentNullException_WhenLogEntryIsNull()
    {
      // Arrange
      ILogFileHandler handler = new LogFileHandler();

      // Act & Assert
      await Assert.ThrowsAsync<ArgumentNullException>(() => handler.AppendLogEntryAsync(null));
    }

    [Fact]
    public async Task AppendLogEntryAsync_ShouldAppendMultipleEntriesInOrder()
    {
      // Arrange
      var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDirectory);
      ILogFileHandler handler = new LogFileHandler(baseDirectory: tempDirectory);

      var logEntry1 = new LogEntry(EventType.START, "First entry");
      var logEntry2 = new LogEntry(EventType.WARN, "Second entry");
      var logEntry3 = new LogEntry(EventType.STOP, "Third entry");

      try
      {
        // Act
        await handler.AppendLogEntryAsync(logEntry1);
        await handler.AppendLogEntryAsync(logEntry2);
        await handler.AppendLogEntryAsync(logEntry3);

        // Assert
        var filePath = handler.GetCurrentLogFilePath();
        var content = await File.ReadAllTextAsync(filePath);
        var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, lines.Length);
        Assert.Contains("First entry", lines[0]);
        Assert.Contains("Second entry", lines[1]);
        Assert.Contains("Third entry", lines[2]);
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
  }
}
