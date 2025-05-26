using System;
using System.Text;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Infrastructure.Services;
using Xunit;

namespace AiDevTest1.Tests
{
  public class MockIoTHubClientTests
  {
    [Fact]
    public async Task GetFileUploadSasUriAsync_WithValidBlobName_ShouldReturnSuccess()
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);
      var blobName = "test-file.log";

      // Act
      var result = await client.GetFileUploadSasUriAsync(blobName);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.NotNull(result.SasUri);
      Assert.NotNull(result.CorrelationId);
      Assert.Null(result.ErrorMessage);
      Assert.Contains(blobName, result.SasUri);
      Assert.True(Guid.TryParse(result.CorrelationId, out _));
    }

    [Fact]
    public async Task GetFileUploadSasUriAsync_WithSimulateFailure_ShouldReturnFailure()
    {
      // Arrange
      var errorMessage = "Test error message";
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: false, errorMessage: errorMessage);
      var blobName = "test-file.log";

      // Act
      var result = await client.GetFileUploadSasUriAsync(blobName);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.Null(result.SasUri);
      Assert.Null(result.CorrelationId);
      Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetFileUploadSasUriAsync_WithInvalidBlobName_ShouldReturnFailure(string? invalidBlobName)
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);

      // Act
      var result = await client.GetFileUploadSasUriAsync(invalidBlobName!);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.Null(result.SasUri);
      Assert.Null(result.CorrelationId);
      Assert.Equal("Blob name cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadToBlobAsync_WithValidParameters_ShouldReturnSuccess()
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);
      var sasUri = "https://mockstorageaccount.blob.core.windows.net/uploads/test.log?sv=2023-01-03&sr=b&sig=mock_signature";
      var fileContent = Encoding.UTF8.GetBytes("Test log content");

      // Act
      var result = await client.UploadToBlobAsync(sasUri, fileContent);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task UploadToBlobAsync_WithSimulateFailure_ShouldReturnFailure()
    {
      // Arrange
      var errorMessage = "Upload failed";
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: false, errorMessage: errorMessage);
      var sasUri = "https://mockstorageaccount.blob.core.windows.net/uploads/test.log";
      var fileContent = Encoding.UTF8.GetBytes("Test log content");

      // Act
      var result = await client.UploadToBlobAsync(sasUri, fileContent);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UploadToBlobAsync_WithInvalidSasUri_ShouldReturnFailure(string? invalidSasUri)
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);
      var fileContent = Encoding.UTF8.GetBytes("Test log content");

      // Act
      var result = await client.UploadToBlobAsync(invalidSasUri!, fileContent);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.Equal("SAS URI cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadToBlobAsync_WithNullFileContent_ShouldReturnFailure()
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);
      var sasUri = "https://mockstorageaccount.blob.core.windows.net/uploads/test.log";

      // Act
      var result = await client.UploadToBlobAsync(sasUri, null!);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.Equal("File content cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadToBlobAsync_WithEmptyFileContent_ShouldReturnFailure()
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);
      var sasUri = "https://mockstorageaccount.blob.core.windows.net/uploads/test.log";
      var emptyFileContent = new byte[0];

      // Act
      var result = await client.UploadToBlobAsync(sasUri, emptyFileContent);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.Equal("File content cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task NotifyFileUploadCompleteAsync_WithValidParameters_ShouldReturnSuccess()
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);
      var correlationId = Guid.NewGuid().ToString();

      // Act
      var result = await client.NotifyFileUploadCompleteAsync(correlationId, true);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task NotifyFileUploadCompleteAsync_WithSimulateFailure_ShouldReturnFailure()
    {
      // Arrange
      var errorMessage = "Notification failed";
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: false, errorMessage: errorMessage);
      var correlationId = Guid.NewGuid().ToString();

      // Act
      var result = await client.NotifyFileUploadCompleteAsync(correlationId, false);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NotifyFileUploadCompleteAsync_WithInvalidCorrelationId_ShouldReturnFailure(string? invalidCorrelationId)
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);

      // Act
      var result = await client.NotifyFileUploadCompleteAsync(invalidCorrelationId!, true);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.Equal("Correlation ID cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task FullWorkflow_WithSuccessScenario_ShouldWorkEndToEnd()
    {
      // Arrange
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: true);
      var blobName = "workflow-test.log";
      var fileContent = Encoding.UTF8.GetBytes("Full workflow test content");

      // Act & Assert
      // Step 1: Get SAS URI
      var sasResult = await client.GetFileUploadSasUriAsync(blobName);
      Assert.True(sasResult.IsSuccess);
      Assert.NotNull(sasResult.SasUri);
      Assert.NotNull(sasResult.CorrelationId);

      // Step 2: Upload to blob
      var uploadResult = await client.UploadToBlobAsync(sasResult.SasUri, fileContent);
      Assert.True(uploadResult.IsSuccess);

      // Step 3: Notify completion
      var notifyResult = await client.NotifyFileUploadCompleteAsync(sasResult.CorrelationId, true);
      Assert.True(notifyResult.IsSuccess);
    }

    [Fact]
    public async Task FullWorkflow_WithFailureScenario_ShouldReturnFailureAtEachStep()
    {
      // Arrange
      var errorMessage = "Workflow failure test";
      IIoTHubClient client = new MockIoTHubClient(simulateSuccess: false, errorMessage: errorMessage);
      var blobName = "workflow-fail-test.log";
      var fileContent = Encoding.UTF8.GetBytes("Failure workflow test content");

      // Act & Assert
      // Step 1: Get SAS URI (should fail)
      var sasResult = await client.GetFileUploadSasUriAsync(blobName);
      Assert.False(sasResult.IsSuccess);
      Assert.Equal(errorMessage, sasResult.ErrorMessage);

      // Step 2: Upload to blob (should fail)
      var dummySasUri = "https://dummy.blob.core.windows.net/test";
      var uploadResult = await client.UploadToBlobAsync(dummySasUri, fileContent);
      Assert.False(uploadResult.IsSuccess);
      Assert.Equal(errorMessage, uploadResult.ErrorMessage);

      // Step 3: Notify completion (should fail)
      var dummyCorrelationId = Guid.NewGuid().ToString();
      var notifyResult = await client.NotifyFileUploadCompleteAsync(dummyCorrelationId, false);
      Assert.False(notifyResult.IsSuccess);
      Assert.Equal(errorMessage, notifyResult.ErrorMessage);
    }
  }
}
