using System;
using AiDevTest1.Domain.ValueObjects;
using Xunit;

namespace AiDevTest1.Tests.ValueObjects
{
  /// <summary>
  /// BlobName Value Objectのユニットテスト
  /// </summary>
  public class BlobNameTests
  {
    #region コンストラクタのテスト

    [Theory]
    [InlineData("test.log")]
    [InlineData("2023-12-25.log")]
    [InlineData("folder/file.txt")]
    [InlineData("folder/subfolder/file.log")]
    [InlineData("file-with-hyphens.txt")]
    [InlineData("file_with_underscores.log")]
    [InlineData("file.with.multiple.dots.txt")]
    [InlineData("UPPERCASE.LOG")]
    [InlineData("MixedCase.Log")]
    [InlineData("123456789.log")]
    [InlineData("a")] // 最小長
    public void Constructor_WithValidBlobName_ShouldCreateInstance(string validName)
    {
      // Act
      var blobName = new BlobName(validName);

      // Assert
      Assert.Equal(validName, blobName.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithNullOrWhiteSpace_ShouldThrowArgumentException(string? invalidName)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new BlobName(invalidName!));
      Assert.Contains("Blob name cannot be null, empty, or whitespace", ex.Message);
      Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithTooLongName_ShouldThrowArgumentException()
    {
      // Arrange
      var tooLongName = new string('a', 1025); // 最大長1024を超える

      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new BlobName(tooLongName));
      Assert.Contains("Blob name length must be between 1 and 1024 characters", ex.Message);
      Assert.Contains("Actual length: 1025", ex.Message);
    }

    [Theory]
    [InlineData("file with spaces.txt")]
    [InlineData("file@name.txt")]
    [InlineData("file#name.txt")]
    [InlineData("file$name.txt")]
    [InlineData("file%name.txt")]
    [InlineData("file&name.txt")]
    [InlineData("file*name.txt")]
    [InlineData("file(name).txt")]
    [InlineData("file[name].txt")]
    [InlineData("file{name}.txt")]
    [InlineData("file<name>.txt")]
    [InlineData("file>name.txt")]
    [InlineData("file|name.txt")]
    [InlineData("file\\name.txt")] // バックスラッシュ
    [InlineData("file\"name.txt")]
    [InlineData("file'name.txt")]
    [InlineData("file:name.txt")]
    [InlineData("file;name.txt")]
    [InlineData("file,name.txt")]
    [InlineData("file?name.txt")]
    [InlineData("file=name.txt")]
    [InlineData("file+name.txt")]
    public void Constructor_WithInvalidCharacters_ShouldThrowArgumentException(string invalidName)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new BlobName(invalidName));
      Assert.Contains("Blob name contains invalid characters", ex.Message);
      Assert.Contains($"Value: '{invalidName}'", ex.Message);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public void Constructor_WithReservedNames_ShouldThrowArgumentException(string reservedName)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new BlobName(reservedName));
      Assert.Contains($"Blob name '{reservedName}' is reserved and cannot be used", ex.Message);
    }

    [Theory]
    [InlineData("folder//file.txt")]
    [InlineData("folder///file.txt")]
    [InlineData("a//b//c.txt")]
    public void Constructor_WithConsecutiveSlashes_ShouldThrowArgumentException(string invalidName)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new BlobName(invalidName));
      Assert.Contains("Blob name cannot contain consecutive forward slashes", ex.Message);
    }

    [Theory]
    [InlineData("/file.txt")]
    [InlineData("file.txt/")]
    [InlineData("/file.txt/")]
    [InlineData("/folder/file.txt")]
    [InlineData("folder/file.txt/")]
    public void Constructor_WithLeadingOrTrailingSlash_ShouldThrowArgumentException(string invalidName)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new BlobName(invalidName));
      Assert.Contains("Blob name cannot start or end with a forward slash", ex.Message);
    }

    [Theory]
    [InlineData(".file.txt")]
    [InlineData("file.txt.")]
    [InlineData("folder/.file.txt")]
    [InlineData("folder/file.txt.")]
    [InlineData(".folder/file.txt")]
    [InlineData("folder./file.txt")]
    public void Constructor_WithSegmentStartingOrEndingWithPeriod_ShouldThrowArgumentException(string invalidName)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new BlobName(invalidName));
      Assert.Contains("Blob name segments cannot start or end with a period", ex.Message);
    }

    #endregion

    #region 暗黙的型変換のテスト

    [Fact]
    public void ImplicitConversion_FromString_ShouldCreateBlobName()
    {
      // Arrange
      string fileName = "test.log";

      // Act
      BlobName blobName = fileName;

      // Assert
      Assert.Equal(fileName, blobName.Value);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
      // Arrange
      var blobName = new BlobName("test.log");

      // Act
      string value = blobName;

      // Assert
      Assert.Equal("test.log", value);
    }

    #endregion

    #region GetFullBlobNameメソッドのテスト

    [Theory]
    [InlineData("device123", "test.log", "device123/test.log")]
    [InlineData("IoT-Device-001", "2023-12-25.log", "IoT-Device-001/2023-12-25.log")]
    [InlineData("sensor_01", "data/readings.csv", "sensor_01/data/readings.csv")]
    public void GetFullBlobName_WithValidDeviceId_ShouldReturnFullPath(string deviceId, string fileName, string expected)
    {
      // Arrange
      var blobName = new BlobName(fileName);

      // Act
      var fullName = blobName.GetFullBlobName(deviceId);

      // Assert
      Assert.Equal(expected, fullName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void GetFullBlobName_WithNullOrEmptyDeviceId_ShouldThrowArgumentException(string? invalidDeviceId)
    {
      // Arrange
      var blobName = new BlobName("test.log");

      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => blobName.GetFullBlobName(invalidDeviceId!));
      Assert.Contains("Device ID cannot be null or empty", ex.Message);
      Assert.Equal("deviceId", ex.ParamName);
    }

    #endregion

    #region ファクトリメソッドのテスト

    [Fact]
    public void CreateForLogFile_WithSpecificDate_ShouldCreateCorrectBlobName()
    {
      // Arrange
      var date = new DateTime(2023, 12, 25);

      // Act
      var blobName = BlobName.CreateForLogFile(date);

      // Assert
      Assert.Equal("2023-12-25.log", blobName.Value);
    }

    [Theory]
    [InlineData(2024, 1, 1, "2024-01-01.log")]
    [InlineData(2023, 12, 31, "2023-12-31.log")]
    [InlineData(2025, 6, 15, "2025-06-15.log")]
    public void CreateForLogFile_WithVariousDates_ShouldCreateCorrectFormat(int year, int month, int day, string expected)
    {
      // Arrange
      var date = new DateTime(year, month, day);

      // Act
      var blobName = BlobName.CreateForLogFile(date);

      // Assert
      Assert.Equal(expected, blobName.Value);
    }

    [Fact]
    public void CreateForTodayLogFile_ShouldCreateBlobNameWithTodayDate()
    {
      // Arrange
      var expectedName = $"{DateTime.Today:yyyy-MM-dd}.log";

      // Act
      var blobName = BlobName.CreateForTodayLogFile();

      // Assert
      Assert.Equal(expectedName, blobName.Value);
    }

    #endregion

    #region ToStringメソッドのテスト

    [Theory]
    [InlineData("test.log")]
    [InlineData("folder/file.txt")]
    [InlineData("2023-12-25.log")]
    public void ToString_ShouldReturnValue(string fileName)
    {
      // Arrange
      var blobName = new BlobName(fileName);

      // Act
      var result = blobName.ToString();

      // Assert
      Assert.Equal(fileName, result);
    }

    #endregion

    #region Equalityのテスト

    [Fact]
    public void Equality_SameBlobNames_ShouldBeEqual()
    {
      // Arrange
      var blobName1 = new BlobName("test.log");
      var blobName2 = new BlobName("test.log");

      // Act & Assert
      Assert.Equal(blobName1, blobName2);
      Assert.True(blobName1 == blobName2);
      Assert.False(blobName1 != blobName2);
      Assert.Equal(blobName1.GetHashCode(), blobName2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentBlobNames_ShouldNotBeEqual()
    {
      // Arrange
      var blobName1 = new BlobName("test1.log");
      var blobName2 = new BlobName("test2.log");

      // Act & Assert
      Assert.NotEqual(blobName1, blobName2);
      Assert.False(blobName1 == blobName2);
      Assert.True(blobName1 != blobName2);
    }

    #endregion

    #region 初期化されていないインスタンスのテスト

    [Fact]
    public void Value_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
      // Arrange
      var blobName = default(BlobName);

      // Act & Assert
      var ex = Assert.Throws<InvalidOperationException>(() => blobName.Value);
      Assert.Equal("BlobName has not been properly initialized.", ex.Message);
    }

    #endregion
  }
}
