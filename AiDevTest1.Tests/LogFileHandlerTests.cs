using AiDevTest1.Infrastructure.Services;
using System;
using System.IO;
using Xunit;

namespace AiDevTest1.Tests
{
  public class LogFileHandlerTests
  {
    [Fact]
    public void GetCurrentLogFilePath_ShouldReturnValidPath()
    {
      // Arrange
      var handler = new LogFileHandler();

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
      var handler = new LogFileHandler();

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
      var handler = new LogFileHandler();
      var expectedBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

      // Act
      var filePath = handler.GetCurrentLogFilePath();

      // Assert
      var actualDirectory = Path.GetDirectoryName(filePath);
      Assert.Equal(expectedBaseDirectory.TrimEnd(Path.DirectorySeparatorChar), actualDirectory);
    }
  }
}
