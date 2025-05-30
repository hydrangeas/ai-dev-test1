using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using AiDevTest1.Infrastructure.Configuration;
using AiDevTest1.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AiDevTest1.Infrastructure.Services
{
  public class FileUploadService : IFileUploadService
  {
    private readonly IIoTHubClient _ioTHubClient;
    private readonly ILogFileHandler _logFileHandler;
    private readonly IRetryPolicy _retryPolicy;

    public FileUploadService(
        IIoTHubClient ioTHubClient,
        ILogFileHandler logFileHandler,
        IRetryPolicy retryPolicy)
    {
      _ioTHubClient = ioTHubClient ?? throw new ArgumentNullException(nameof(ioTHubClient));
      _logFileHandler = logFileHandler ?? throw new ArgumentNullException(nameof(logFileHandler));
      _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
    }

    /// <summary>
    /// ログファイルをIoT Hub経由でBlobストレージにアップロードします
    /// RetryPolicyを使用してリトライ機能を提供します
    /// </summary>
    /// <returns>アップロード操作の結果</returns>
    public async Task<Result> UploadLogFileAsync()
    {
      try
      {
        // 1. LogFileHandlerからファイルパスを取得
        var logFilePath = _logFileHandler.GetCurrentLogFilePath();

        if (!logFilePath.Exists())
        {
          return Result.Failure($"ログファイルが存在しません: {logFilePath}");
        }

        // 2. ファイル内容を読み取り
        byte[] fileContent;
        try
        {
          // LogFilePathからstringへの暗黙的な変換を利用
          fileContent = await File.ReadAllBytesAsync(logFilePath);
        }
        catch (Exception ex)
        {
          return Result.Failure($"ログファイルの読み取りに失敗しました: {ex.Message}");
        }

        // 3. blob名を生成 (yyyy-MM-dd.log形式)
        BlobName blobName = logFilePath.FileName; // 暗黙的な型変換でBlobNameに変換

        // 4. RetryPolicyを使用してアップロード処理を実行
        var result = await _retryPolicy.ExecuteAsync(async () =>
        {
          return await PerformFileUploadAsync(blobName, fileContent);
        });

        return result;
      }
      catch (AggregateException ex)
      {
        // RetryPolicyからのAggregateExceptionを処理
        var errorMessages = new List<string>();
        foreach (var innerEx in ex.InnerExceptions)
        {
          errorMessages.Add(innerEx.Message);
        }

        var combinedErrorMessage = $"ファイルアップロードが全ての試行で失敗しました:\n{string.Join("\n", errorMessages)}";
        return Result.Failure(combinedErrorMessage);
      }
      catch (Exception ex)
      {
        return Result.Failure($"ファイルアップロード処理で予期しないエラーが発生しました: {ex.Message}");
      }
    }

    /// <summary>
    /// 単一のファイルアップロード操作を実行します
    /// この操作はRetryPolicyによってリトライされる可能性があります
    /// </summary>
    /// <param name="blobName">アップロード先のBlob名</param>
    /// <param name="fileContent">アップロードするファイルの内容</param>
    /// <returns>アップロード操作の結果</returns>
    private async Task<Result> PerformFileUploadAsync(BlobName blobName, byte[] fileContent)
    {
      // SAS URI取得
      var sasUriResult = await _ioTHubClient.GetFileUploadSasUriAsync(blobName);
      if (sasUriResult.IsFailure)
      {
        throw new InvalidOperationException($"SAS URI取得に失敗: {sasUriResult.ErrorMessage}");
      }

      // SAS URIの検証
      if (string.IsNullOrEmpty(sasUriResult.SasUri))
      {
        throw new InvalidOperationException("SAS URIが空です");
      }

      // Blobアップロード実行
      var uploadResult = await _ioTHubClient.UploadToBlobAsync(sasUriResult.SasUri, fileContent);
      if (uploadResult.IsFailure)
      {
        // 失敗した場合は失敗を通知してから例外をスロー
        if (!string.IsNullOrEmpty(sasUriResult.CorrelationId))
        {
          await _ioTHubClient.NotifyFileUploadCompleteAsync(sasUriResult.CorrelationId, false);
        }
        throw new InvalidOperationException($"Blobアップロードに失敗: {uploadResult.ErrorMessage}");
      }

      // 成功時の処理
      if (string.IsNullOrEmpty(sasUriResult.CorrelationId))
      {
        return Result.Failure("相関IDが空です");
      }

      // アップロード成功を通知
      var notifyResult = await _ioTHubClient.NotifyFileUploadCompleteAsync(sasUriResult.CorrelationId, true);
      if (notifyResult.IsFailure)
      {
        return Result.Failure($"アップロード完了通知に失敗しました: {notifyResult.ErrorMessage}");
      }

      return Result.Success();
    }
  }
}
