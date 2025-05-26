using System;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;

namespace AiDevTest1.Infrastructure.Services;

/// <summary>
/// IIoTHubClientのモック実装クラス
/// 開発時のテストやデバッグに使用
/// </summary>
public class MockIoTHubClient : IIoTHubClient
{
  private readonly bool _simulateSuccess;
  private readonly string _errorMessage;

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="simulateSuccess">成功をシミュレートするかどうか（デフォルト: true）</param>
  /// <param name="errorMessage">失敗時のエラーメッセージ（デフォルト: "Simulated failure"）</param>
  public MockIoTHubClient(bool simulateSuccess = true, string errorMessage = "Simulated failure")
  {
    _simulateSuccess = simulateSuccess;
    _errorMessage = errorMessage;
  }

  /// <summary>
  /// ファイルアップロード用のSAS URIを取得します（モック実装）
  /// </summary>
  /// <param name="blobName">アップロードするファイルのBlob名</param>
  /// <returns>SAS URIと相関IDを含む結果</returns>
  public Task<SasUriResult> GetFileUploadSasUriAsync(string blobName)
  {
    if (string.IsNullOrWhiteSpace(blobName))
    {
      return Task.FromResult(SasUriResult.Failure("Blob name cannot be null or empty"));
    }

    if (!_simulateSuccess)
    {
      return Task.FromResult(SasUriResult.Failure(_errorMessage));
    }

    // 成功時のモックデータを生成
    var correlationId = Guid.NewGuid().ToString();
    var sasUri = $"https://mockstorageaccount.blob.core.windows.net/uploads/{blobName}?sv=2023-01-03&sr=b&sig=mock_signature&se=2024-12-31T23:59:59Z&sp=w";

    return Task.FromResult(SasUriResult.Success(sasUri, correlationId));
  }

  /// <summary>
  /// 指定されたSAS URIを使用してファイルをBlobストレージにアップロードします（モック実装）
  /// </summary>
  /// <param name="sasUri">SAS URI</param>
  /// <param name="fileContent">アップロードするファイルの内容</param>
  /// <returns>アップロード操作の結果</returns>
  public Task<UploadToBlobResult> UploadToBlobAsync(string sasUri, byte[] fileContent)
  {
    if (string.IsNullOrWhiteSpace(sasUri))
    {
      return Task.FromResult(UploadToBlobResult.Failure("SAS URI cannot be null or empty"));
    }

    if (fileContent == null || fileContent.Length == 0)
    {
      return Task.FromResult(UploadToBlobResult.Failure("File content cannot be null or empty"));
    }

    if (!_simulateSuccess)
    {
      return Task.FromResult(UploadToBlobResult.Failure(_errorMessage));
    }

    // 成功時のシミュレーション（実際にはアップロードしない）
    Console.WriteLine($"[MockIoTHubClient]: Simulated blob upload to {sasUri}, size: {fileContent.Length} bytes");

    return Task.FromResult(UploadToBlobResult.Success());
  }

  /// <summary>
  /// ファイルアップロード完了をIoT Hubに通知します（モック実装）
  /// </summary>
  /// <param name="correlationId">相関ID</param>
  /// <param name="isSuccess">アップロードが成功したかどうか</param>
  /// <returns>通知操作の結果</returns>
  public Task<Result> NotifyFileUploadCompleteAsync(string correlationId, bool isSuccess)
  {
    if (string.IsNullOrWhiteSpace(correlationId))
    {
      return Task.FromResult(Result.Failure("Correlation ID cannot be null or empty"));
    }

    if (!_simulateSuccess)
    {
      return Task.FromResult(Result.Failure(_errorMessage));
    }

    // 成功時のシミュレーション
    Console.WriteLine($"[MockIoTHubClient]: Simulated upload completion notification - CorrelationId: {correlationId}, Success: {isSuccess}");

    return Task.FromResult(Result.Success());
  }
}
