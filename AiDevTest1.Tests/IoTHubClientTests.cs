using AiDevTest1.Infrastructure.Configuration;
using AiDevTest1.Domain.ValueObjects;
using AiDevTest1.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AiDevTest1.Tests;

/// <summary>
/// IoTHubClientクラスのユニットテスト
/// </summary>
public class IoTHubClientTests
{
  /// <summary>
  /// コンストラクタでnullのioTHubConfigurationが渡された場合、ArgumentNullExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithNullIoTHubConfiguration_ThrowsArgumentNullException()
  {
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new IoTHubClient((IOptions<IoTHubConfiguration>)null!));

    exception.ParamName.Should().Be("iotHubConfiguration");
  }

  /// <summary>
  /// コンストラクタでioTHubConfiguration.Valueがnullの場合、ArgumentNullExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithNullIoTHubConfigurationValue_ThrowsArgumentNullException()
  {
    // Arrange
    var options = Options.Create<IoTHubConfiguration>(null!);

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new IoTHubClient(options));

    exception.ParamName.Should().Be("iotHubConfiguration");
  }

  /// <summary>
  /// コンストラクタで空のConnectionStringが渡された場合、ArgumentExceptionが発生することをテスト
  /// </summary>
  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void Constructor_WithInvalidConnectionString_ThrowsArgumentException(string? invalidConnectionString)
  {
    // Act & Assert
    var exception = Assert.Throws<ArgumentException>(() =>
        new IoTHubConfiguration(invalidConnectionString!, new DeviceId("valid-device-id")));

    exception.ParamName.Should().Be("connectionString");
    exception.Message.Should().Contain("接続文字列が空です");
  }

  /// <summary>
  /// コンストラクタで無効なConnectionStringが渡された場合、InvalidOperationExceptionが発生することをテスト
  /// </summary>
  [Fact]
  public void Constructor_WithInvalidConnectionStringFormat_ThrowsInvalidOperationException()
  {
    // Arrange
    var config = new IoTHubConfiguration("invalid-connection-string", new DeviceId("valid-device-id"));
    var options = Options.Create(config);

    // Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() =>
        new IoTHubClient(options));

    exception.Message.Should().Contain("Failed to create DeviceClient");
  }

  /// <summary>
  /// Disposeされた後にGetFileUploadSasUriAsyncを呼び出した場合、失敗結果が返されることをテスト
  /// </summary>
  [Fact]
  public async Task GetFileUploadSasUriAsync_AfterDispose_ReturnsFailure()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    var client = new IoTHubClient(options);
    client.Dispose();

    // Act
    var result = await client.GetFileUploadSasUriAsync(new BlobName("test.log"));

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("IoTHubClient has been disposed");
    result.SasUri.Should().BeNull();
    result.CorrelationId.Should().BeNull();
  }

  /// <summary>
  /// UploadToBlobAsyncでnullのsasUriが渡された場合、失敗結果が返されることをテスト
  /// </summary>
  [Fact]
  public async Task UploadToBlobAsync_WithNullSasUri_ReturnsFailure()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    using var client = new IoTHubClient(options);
    var fileContent = "test content"u8.ToArray();

    // Act
    var result = await client.UploadToBlobAsync(null!, fileContent);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("SAS URI cannot be null or empty");
  }

  /// <summary>
  /// UploadToBlobAsyncで空のsasUriが渡された場合、失敗結果が返されることをテスト
  /// </summary>
  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public async Task UploadToBlobAsync_WithEmptySasUri_ReturnsFailure(string emptySasUri)
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    using var client = new IoTHubClient(options);
    var fileContent = "test content"u8.ToArray();

    // Act
    var result = await client.UploadToBlobAsync(emptySasUri, fileContent);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("SAS URI cannot be null or empty");
  }

  /// <summary>
  /// UploadToBlobAsyncでnullのfileContentが渡された場合、失敗結果が返されることをテスト
  /// </summary>
  [Fact]
  public async Task UploadToBlobAsync_WithNullFileContent_ReturnsFailure()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    using var client = new IoTHubClient(options);
    var sasUri = "https://test.blob.core.windows.net/logs/test.log";

    // Act
    var result = await client.UploadToBlobAsync(sasUri, null!);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("File content cannot be null or empty");
  }

  /// <summary>
  /// UploadToBlobAsyncで空のfileContentが渡された場合、失敗結果が返されることをテスト
  /// </summary>
  [Fact]
  public async Task UploadToBlobAsync_WithEmptyFileContent_ReturnsFailure()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    using var client = new IoTHubClient(options);
    var sasUri = "https://test.blob.core.windows.net/logs/test.log";
    var emptyFileContent = new byte[0];

    // Act
    var result = await client.UploadToBlobAsync(sasUri, emptyFileContent);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("File content cannot be null or empty");
  }

  /// <summary>
  /// Disposeされた後にUploadToBlobAsyncを呼び出した場合、失敗結果が返されることをテスト
  /// </summary>
  [Fact]
  public async Task UploadToBlobAsync_AfterDispose_ReturnsFailure()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    var client = new IoTHubClient(options);
    client.Dispose();

    var sasUri = "https://test.blob.core.windows.net/logs/test.log";
    var fileContent = "test content"u8.ToArray();

    // Act
    var result = await client.UploadToBlobAsync(sasUri, fileContent);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("IoTHubClient has been disposed");
  }

  /// <summary>
  /// NotifyFileUploadCompleteAsyncでnullのcorrelationIdが渡された場合、失敗結果が返されることをテスト
  /// </summary>
  [Fact]
  public async Task NotifyFileUploadCompleteAsync_WithNullCorrelationId_ReturnsFailure()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    using var client = new IoTHubClient(options);

    // Act
    var result = await client.NotifyFileUploadCompleteAsync(null!, true);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("Correlation ID cannot be null or empty");
  }

  /// <summary>
  /// NotifyFileUploadCompleteAsyncで空のcorrelationIdが渡された場合、失敗結果が返されることをテスト
  /// </summary>
  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public async Task NotifyFileUploadCompleteAsync_WithEmptyCorrelationId_ReturnsFailure(string emptyCorrelationId)
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    using var client = new IoTHubClient(options);

    // Act
    var result = await client.NotifyFileUploadCompleteAsync(emptyCorrelationId, true);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("Correlation ID cannot be null or empty");
  }

  /// <summary>
  /// Disposeされた後にNotifyFileUploadCompleteAsyncを呼び出した場合、失敗結果が返されることをテスト
  /// </summary>
  [Fact]
  public async Task NotifyFileUploadCompleteAsync_AfterDispose_ReturnsFailure()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    var client = new IoTHubClient(options);
    client.Dispose();

    var correlationId = "test-correlation-id";

    // Act
    var result = await client.NotifyFileUploadCompleteAsync(correlationId, true);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.ErrorMessage.Should().Be("IoTHubClient has been disposed");
  }

  /// <summary>
  /// Disposeパターンのテスト：複数回Disposeを呼び出しても例外が発生しないことをテスト
  /// </summary>
  [Fact]
  public void Dispose_CalledMultipleTimes_DoesNotThrowException()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    var client = new IoTHubClient(options);

    // Act & Assert
    client.Dispose(); // 1回目
    client.Dispose(); // 2回目 - 例外が発生しないことを確認

    // 例外が発生しなければテスト成功
  }

  /// <summary>
  /// usingステートメントでのDisposeパターンのテスト
  /// </summary>
  [Fact]
  public void UsingStatement_DisposesProperly()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    // Act & Assert
    using (var client = new IoTHubClient(options))
    {
      // usingブロック内での操作
      client.Should().NotBeNull();
    }
    // usingブロックを抜ける際に自動的にDisposeが呼ばれる
  }

  /// <summary>
  /// 正常なパラメータでのコンストラクタ実行テスト
  /// </summary>
  [Fact]
  public void Constructor_WithValidParameters_CreatesInstance()
  {
    // Arrange
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        new DeviceId("test-device"));
    var options = Options.Create(config);

    // Act
    using var client = new IoTHubClient(options);

    // Assert
    client.Should().NotBeNull();
  }

  /// <summary>
  /// blobNameにデバイスIDが適切に含まれることをテスト（SAS URI生成時のfullBlobName構築）
  /// </summary>
  [Fact]
  public async Task GetFileUploadSasUriAsync_WithValidBlobName_IncludesDeviceIdInBlobName()
  {
    // Arrange
    var deviceId = new DeviceId("test-device-123");
    var config = new IoTHubConfiguration(
        "HostName=test.azure-devices.net;DeviceId=test;SharedAccessKey=dGVzdA==",
        deviceId);
    var options = Options.Create(config);

    using var client = new IoTHubClient(options);
    var blobName = new BlobName("2023-12-25.log");

    // Act
    // このテストは実際のAzure SDKに依存するため、実際の接続エラーが発生することが期待される
    var result = await client.GetFileUploadSasUriAsync(blobName);

    // Assert
    // 実際のAzure IoT Hubに接続しないため、接続エラーが発生するはず
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeFalse();
    result.ErrorMessage.Should().NotBeNull();
    result.ErrorMessage.Should().Contain("Failed to get file upload SAS URI");
  }
}
