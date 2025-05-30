using AiDevTest1.Domain.Exceptions;
using AiDevTest1.Domain.ValueObjects;
using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace AiDevTest1.Tests.ValueObjects
{
  /// <summary>
  /// LogFilePath Value Objectのユニットテスト
  /// </summary>
  public class LogFilePathTests
  {
    [Fact]
    public void Constructor_WithValidPath_CreatesInstance()
    {
      // Arrange
      var path = Path.Combine(Path.GetTempPath(), "test.log");

      // Act
      var logFilePath = new LogFilePath(path);

      // Assert
      logFilePath.Value.Should().NotBeNull();
      logFilePath.Value.Should().EndWith("test.log");
      Path.IsPathFullyQualified(logFilePath.Value).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyPath_ThrowsDomainException(string? invalidPath)
    {
      // Act & Assert
      var exception = Assert.Throws<DomainException>(() => new LogFilePath(invalidPath!));
      exception.Message.Should().Contain("ログファイルパスが空です");
      exception.ErrorCode.Should().Be("LogFilePath.Empty");
    }

    [Theory]
    [InlineData("test.txt")]
    [InlineData("test")]
    [InlineData("test.")]
    public void Constructor_WithInvalidExtension_ThrowsDomainException(string invalidPath)
    {
      // Arrange
      var fullPath = Path.Combine(Path.GetTempPath(), invalidPath);

      // Act & Assert
      var exception = Assert.Throws<DomainException>(() => new LogFilePath(fullPath));
      exception.Message.Should().Contain("ログファイルは.log拡張子である必要があります");
      exception.ErrorCode.Should().Be("LogFilePath.InvalidExtension");
    }

    [Theory]
    [InlineData("test.LOG")]
    [InlineData("test.Log")]
    [InlineData("test.LoG")]
    public void Constructor_WithUpperCaseLogExtension_DoesNotThrow(string validPath)
    {
      // Arrange
      var fullPath = Path.Combine(Path.GetTempPath(), validPath);

      // Act
      var logFilePath = new LogFilePath(fullPath);

      // Assert
      logFilePath.Value.Should().NotBeNull();
      Path.GetExtension(logFilePath.Value).Should().BeOneOf(".log", ".LOG", ".Log", ".LoG");
    }

    [Theory]
    [InlineData("C:\0invalid*path?.log")] // 無効な文字を含む
    [InlineData("C:\0test\0\0.log")] // null文字を含む
    public void Constructor_WithInvalidPathFormat_ThrowsDomainException(string invalidPath)
    {
      // Act & Assert
      var exception = Assert.Throws<DomainException>(() => new LogFilePath(invalidPath));
      exception.Message.Should().Contain("無効なログファイルパス形式です");
      exception.ErrorCode.Should().Be("LogFilePath.InvalidFormat");
      exception.InnerException.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithRelativePath_ConvertsToAbsolutePath()
    {
      // Arrange
      var relativePath = "logs/test.log";

      // Act
      var logFilePath = new LogFilePath(relativePath);

      // Assert
      Path.IsPathFullyQualified(logFilePath.Value).Should().BeTrue();
    }

    [Fact]
    public void FileName_ReturnsCorrectFileName()
    {
      // Arrange
      var path = Path.Combine(Path.GetTempPath(), "2023-12-25.log");
      var logFilePath = new LogFilePath(path);

      // Act
      var fileName = logFilePath.FileName;

      // Assert
      fileName.Should().Be("2023-12-25.log");
    }

    [Fact]
    public void DirectoryPath_ReturnsCorrectDirectory()
    {
      // Arrange
      var tempPath = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
      var path = Path.Combine(tempPath, "2023-12-25.log");
      var logFilePath = new LogFilePath(path);

      // Act
      var directoryPath = logFilePath.DirectoryPath;

      // Assert
      directoryPath.Should().Be(tempPath);
    }

    [Fact]
    public void Exists_WhenFileExists_ReturnsTrue()
    {
      // Arrange
      var tempFile = Path.GetTempFileName();
      File.Move(tempFile, tempFile.Replace(".tmp", ".log"));
      tempFile = tempFile.Replace(".tmp", ".log");

      try
      {
        var logFilePath = new LogFilePath(tempFile);

        // Act
        var exists = logFilePath.Exists();

        // Assert
        exists.Should().BeTrue();
      }
      finally
      {
        if (File.Exists(tempFile))
        {
          File.Delete(tempFile);
        }
      }
    }

    [Fact]
    public void Exists_WhenFileDoesNotExist_ReturnsFalse()
    {
      // Arrange
      var path = Path.Combine(Path.GetTempPath(), "non-existent.log");
      var logFilePath = new LogFilePath(path);

      // Act
      var exists = logFilePath.Exists();

      // Assert
      exists.Should().BeFalse();
    }

    [Fact]
    public void EnsureDirectoryExists_CreatesDirectoryIfNotExists()
    {
      // Arrange
      var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      var path = Path.Combine(tempDirectory, "test.log");
      var logFilePath = new LogFilePath(path);

      try
      {
        // Act
        logFilePath.EnsureDirectoryExists();

        // Assert
        Directory.Exists(tempDirectory).Should().BeTrue();
      }
      finally
      {
        if (Directory.Exists(tempDirectory))
        {
          Directory.Delete(tempDirectory, true);
        }
      }
    }

    [Fact]
    public void ImplicitConversion_FromString_WorksCorrectly()
    {
      // Arrange
      string path = Path.Combine(Path.GetTempPath(), "test.log");

      // Act
      LogFilePath logFilePath = path;

      // Assert
      logFilePath.Value.Should().EndWith("test.log");
    }

    [Fact]
    public void ImplicitConversion_ToString_WorksCorrectly()
    {
      // Arrange
      var path = Path.Combine(Path.GetTempPath(), "test.log");
      var logFilePath = new LogFilePath(path);

      // Act
      string convertedPath = logFilePath;

      // Assert
      convertedPath.Should().Be(logFilePath.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
      // Arrange
      var path = Path.Combine(Path.GetTempPath(), "test.log");
      var logFilePath = new LogFilePath(path);

      // Act
      var result = logFilePath.ToString();

      // Assert
      result.Should().Be(logFilePath.Value);
    }

    [Fact]
    public void CreateForDate_WithValidInputs_CreatesCorrectPath()
    {
      // Arrange
      var baseDirectory = Path.GetTempPath();
      var date = new DateTime(2023, 12, 25);

      // Act
      var logFilePath = LogFilePath.CreateForDate(baseDirectory, date);

      // Assert
      logFilePath.FileName.Should().Be("2023-12-25.log");
      logFilePath.DirectoryPath.Should().Be(baseDirectory.TrimEnd(Path.DirectorySeparatorChar));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateForDate_WithInvalidBaseDirectory_ThrowsArgumentException(string? invalidDirectory)
    {
      // Arrange
      var date = DateTime.Now;

      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => LogFilePath.CreateForDate(invalidDirectory!, date));
      exception.Message.Should().Contain("ベースディレクトリが指定されていません");
      exception.ParamName.Should().Be("baseDirectory");
    }

    [Fact]
    public void CreateForToday_CreatesPathWithTodaysDate()
    {
      // Arrange
      var baseDirectory = Path.GetTempPath();
      var today = DateTime.Now;

      // Act
      var logFilePath = LogFilePath.CreateForToday(baseDirectory);

      // Assert
      logFilePath.FileName.Should().Be($"{today:yyyy-MM-dd}.log");
      logFilePath.DirectoryPath.Should().Be(baseDirectory.TrimEnd(Path.DirectorySeparatorChar));
    }

    [Fact]
    public void Equality_WithSamePath_AreEqual()
    {
      // Arrange
      var path = Path.Combine(Path.GetTempPath(), "test.log");
      var logFilePath1 = new LogFilePath(path);
      var logFilePath2 = new LogFilePath(path);

      // Act & Assert
      logFilePath1.Should().Be(logFilePath2);
      (logFilePath1 == logFilePath2).Should().BeTrue();
      logFilePath1.GetHashCode().Should().Be(logFilePath2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentPath_AreNotEqual()
    {
      // Arrange
      var logFilePath1 = new LogFilePath(Path.Combine(Path.GetTempPath(), "test1.log"));
      var logFilePath2 = new LogFilePath(Path.Combine(Path.GetTempPath(), "test2.log"));

      // Act & Assert
      logFilePath1.Should().NotBe(logFilePath2);
      (logFilePath1 != logFilePath2).Should().BeTrue();
    }

    [Fact]
    public void Value_WhenNotInitialized_ThrowsInvalidOperationException()
    {
      // Arrange
      var logFilePath = default(LogFilePath);

      // Act & Assert
      var exception = Assert.Throws<InvalidOperationException>(() => logFilePath.Value);
      exception.Message.Should().Contain("LogFilePath has not been properly initialized");
    }
  }
}
