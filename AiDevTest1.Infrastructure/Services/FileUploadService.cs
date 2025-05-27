using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using System;
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

        // 4. IIoTHubClientを使用してSAS URIを取得
        var sasUriResult = await _ioTHubClient.GetFileUploadSasUriAsync(blobName);
        if (sasUriResult.IsFailure)
        {
          return Result.Failure($"SAS URI取得に失敗しました: {sasUriResult.ErrorMessage}");
        }

        // 5. 取得したSAS URIとファイル内容でアップロードを実行
        if (string.IsNullOrEmpty(sasUriResult.SasUri))
        {
          return Result.Failure("SAS URIが空です");
        }

        var uploadResult = await _ioTHubClient.UploadToBlobAsync(sasUriResult.SasUri, fileContent);
        if (uploadResult.IsFailure)
        {
          // アップロード失敗を通知
          if (!string.IsNullOrEmpty(sasUriResult.CorrelationId))
          {
            await _ioTHubClient.NotifyFileUploadCompleteAsync(sasUriResult.CorrelationId, false);
          }
          return Result.Failure($"ファイルアップロードに失敗しました: {uploadResult.ErrorMessage}");
        }

        // 6. アップロード成功を通知
        if (string.IsNullOrEmpty(sasUriResult.CorrelationId))
        {
          return Result.Failure("相関IDが空です");
        }

        var notifyResult = await _ioTHubClient.NotifyFileUploadCompleteAsync(sasUriResult.CorrelationId, true);
        if (notifyResult.IsFailure)
        {
          return Result.Failure($"アップロード完了通知に失敗しました: {notifyResult.ErrorMessage}");
        }

        return Result.Success();
      }
      catch (Exception ex)
      {
        return Result.Failure($"予期しないエラーが発生しました: {ex.Message}");
      }
    }
  }
}
