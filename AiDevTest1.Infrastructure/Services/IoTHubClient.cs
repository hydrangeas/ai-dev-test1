using System;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using AiDevTest1.Domain.ValueObjects;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport;
using Microsoft.Extensions.Options;

namespace AiDevTest1.Infrastructure.Services;

/// <summary>
/// Azure IoT Hub Device SDKを使用したIIoTHubClientの実装
/// </summary>
public class IoTHubClient : IIoTHubClient, IDisposable
{
  private readonly DeviceClient _deviceClient;
  private readonly string _deviceId;
  private bool _disposed = false;

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="authenticationInfo">認証情報</param>
  public IoTHubClient(IOptions<AuthenticationInfo> authenticationInfo)
  {
    if (authenticationInfo?.Value == null)
    {
      throw new ArgumentNullException(nameof(authenticationInfo));
    }

    var authInfo = authenticationInfo.Value;

    if (string.IsNullOrWhiteSpace(authInfo.ConnectionString))
    {
      throw new ArgumentException("Connection string is required", nameof(authenticationInfo));
    }

    if (string.IsNullOrWhiteSpace(authInfo.DeviceId))
    {
      throw new ArgumentException("Device ID is required", nameof(authenticationInfo));
    }

    _deviceId = authInfo.DeviceId;

    try
    {
      // DeviceClientを初期化
      _deviceClient = DeviceClient.CreateFromConnectionString(
          authInfo.ConnectionString,
          TransportType.Mqtt);
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to create DeviceClient: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// ファイルアップロード用のSAS URIを取得します
  /// </summary>
  /// <param name="blobName">アップロードするファイルのBlob名</param>
  /// <returns>SAS URIと相関IDを含む結果</returns>
  public async Task<SasUriResult> GetFileUploadSasUriAsync(BlobName blobName)
  {
    if (_disposed)
    {
      return SasUriResult.Failure("IoTHubClient has been disposed");
    }

    try
    {
      // デバイスID/ファイル名の形式でBlob名を構築
      var fullBlobName = blobName.GetFullBlobName(_deviceId);

      // Azure IoT Hub SDKを使用してファイルアップロード用のSAS URIを取得
      var fileUploadSasUriRequest = new FileUploadSasUriRequest
      {
        BlobName = fullBlobName
      };

      var fileUploadSasUriResponse = await _deviceClient.GetFileUploadSasUriAsync(fileUploadSasUriRequest);

      if (fileUploadSasUriResponse == null)
      {
        return SasUriResult.Failure("Failed to get file upload SAS URI response");
      }

      return SasUriResult.Success(
          fileUploadSasUriResponse.GetBlobUri().ToString(),
          fileUploadSasUriResponse.CorrelationId);
    }
    catch (Exception ex)
    {
      return SasUriResult.Failure($"Failed to get file upload SAS URI: {ex.Message}");
    }
  }

  /// <summary>
  /// 指定されたSAS URIを使用してファイルをBlobストレージにアップロードします
  /// </summary>
  /// <param name="sasUri">SAS URI</param>
  /// <param name="fileContent">アップロードするファイルの内容</param>
  /// <returns>アップロード操作の結果</returns>
  public async Task<UploadToBlobResult> UploadToBlobAsync(string sasUri, byte[] fileContent)
  {
    if (string.IsNullOrWhiteSpace(sasUri))
    {
      return UploadToBlobResult.Failure("SAS URI cannot be null or empty");
    }

    if (fileContent == null || fileContent.Length == 0)
    {
      return UploadToBlobResult.Failure("File content cannot be null or empty");
    }

    if (_disposed)
    {
      return UploadToBlobResult.Failure("IoTHubClient has been disposed");
    }

    try
    {
      // SAS URIを使用してBlobにアップロード
      using var stream = new System.IO.MemoryStream(fileContent);

      // Azure Storage Blob Clientを使用してアップロード
      var blobUri = new Uri(sasUri);
      var blobClient = new Azure.Storage.Blobs.BlobClient(blobUri);

      await blobClient.UploadAsync(stream, overwrite: true);

      return UploadToBlobResult.Success();
    }
    catch (Exception ex)
    {
      return UploadToBlobResult.Failure($"Failed to upload file to blob storage: {ex.Message}");
    }
  }

  /// <summary>
  /// ファイルアップロード完了をIoT Hubに通知します
  /// </summary>
  /// <param name="correlationId">相関ID</param>
  /// <param name="isSuccess">アップロードが成功したかどうか</param>
  /// <returns>通知操作の結果</returns>
  public async Task<Result> NotifyFileUploadCompleteAsync(string correlationId, bool isSuccess)
  {
    if (string.IsNullOrWhiteSpace(correlationId))
    {
      return Result.Failure("Correlation ID cannot be null or empty");
    }

    if (_disposed)
    {
      return Result.Failure("IoTHubClient has been disposed");
    }

    try
    {
      // ファイルアップロード完了通知を作成
      var fileUploadCompletionNotification = new FileUploadCompletionNotification
      {
        CorrelationId = correlationId,
        IsSuccess = isSuccess,
        StatusCode = isSuccess ? 200 : 500,
        StatusDescription = isSuccess ? "Success" : "Upload failed"
      };

      // IoT Hubにファイルアップロード完了を通知
      await _deviceClient.CompleteFileUploadAsync(fileUploadCompletionNotification);

      return Result.Success();
    }
    catch (Exception ex)
    {
      return Result.Failure($"Failed to notify file upload completion: {ex.Message}");
    }
  }

  /// <summary>
  /// リソースを解放します
  /// </summary>
  public void Dispose()
  {
    if (!_disposed)
    {
      _deviceClient?.Dispose();
      _disposed = true;
    }
  }
}
