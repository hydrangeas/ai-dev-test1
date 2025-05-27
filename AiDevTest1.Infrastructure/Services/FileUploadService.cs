using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
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
    private readonly AuthenticationInfo _authInfo;

    public FileUploadService(
        IIoTHubClient ioTHubClient,
        ILogFileHandler logFileHandler,
        AuthenticationInfo authInfo)
    {
      _ioTHubClient = ioTHubClient ?? throw new ArgumentNullException(nameof(ioTHubClient));
      _logFileHandler = logFileHandler ?? throw new ArgumentNullException(nameof(logFileHandler));
      _authInfo = authInfo ?? throw new ArgumentNullException(nameof(authInfo));
    }

    /// <summary>
    /// ログファイルをIoT Hub経由でBlobストレージにアップロードします
    /// リトライ機能付き（最大3回、間隔5秒）
    /// </summary>
    /// <returns>アップロード操作の結果</returns>
    public async Task<Result> UploadLogFileAsync()
    {
      try
      {
        // 1. LogFileHandlerからファイルパスを取得
        var logFilePath = _logFileHandler.GetCurrentLogFilePath();

        if (!File.Exists(logFilePath))
        {
          return Result.Failure($"ログファイルが存在しません: {logFilePath}");
        }

        // 2. ファイル内容を読み取り
        byte[] fileContent;
        try
        {
          fileContent = await File.ReadAllBytesAsync(logFilePath);
        }
        catch (Exception ex)
        {
          return Result.Failure($"ログファイルの読み取りに失敗しました: {ex.Message}");
        }

        // 3. blob名を生成 (yyyy-MM-dd.log形式)
        var fileName = Path.GetFileName(logFilePath);
        var blobName = fileName;

        // 4. リトライロジック付きでアップロード処理を実行
        const int maxRetries = 3;
        const int retryDelayMs = 5000; // 5秒
        var attemptErrors = new List<string>();

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
          try
          {
            // SAS URI取得
            var sasUriResult = await _ioTHubClient.GetFileUploadSasUriAsync(blobName);
            if (sasUriResult.IsFailure)
            {
              var error = $"試行{attempt}: SAS URI取得に失敗 - {sasUriResult.ErrorMessage}";
              attemptErrors.Add(error);

              if (attempt == maxRetries)
              {
                break; // 最後の試行なのでループを抜ける
              }

              await Task.Delay(retryDelayMs);
              continue;
            }

            // SAS URIの検証
            if (string.IsNullOrEmpty(sasUriResult.SasUri))
            {
              var error = $"試行{attempt}: SAS URIが空";
              attemptErrors.Add(error);

              if (attempt == maxRetries)
              {
                break;
              }

              await Task.Delay(retryDelayMs);
              continue;
            }

            // Blobアップロード実行
            var uploadResult = await _ioTHubClient.UploadToBlobAsync(sasUriResult.SasUri, fileContent);
            if (uploadResult.IsFailure)
            {
              var error = $"試行{attempt}: Blobアップロードに失敗 - {uploadResult.ErrorMessage}";
              attemptErrors.Add(error);

              if (attempt == maxRetries)
              {
                // 最後の試行で失敗した場合、失敗を通知
                if (!string.IsNullOrEmpty(sasUriResult.CorrelationId))
                {
                  await _ioTHubClient.NotifyFileUploadCompleteAsync(sasUriResult.CorrelationId, false);
                }
                break;
              }

              await Task.Delay(retryDelayMs);
              continue;
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
          catch (Exception ex)
          {
            var error = $"試行{attempt}: 予期しないエラー - {ex.Message}";
            attemptErrors.Add(error);

            if (attempt == maxRetries)
            {
              break;
            }

            await Task.Delay(retryDelayMs);
          }
        }

        // 全ての試行が失敗した場合
        var combinedErrorMessage = $"ファイルアップロードが{maxRetries}回の試行すべてで失敗しました。\n" +
                                   string.Join("\n", attemptErrors);
        return Result.Failure(combinedErrorMessage);
      }
      catch (Exception ex)
      {
        return Result.Failure($"ファイルアップロード処理で予期しないエラーが発生しました: {ex.Message}");
      }
    }
  }
}
