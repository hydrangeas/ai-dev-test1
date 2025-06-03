using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Commands;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.ValueObjects;

namespace AiDevTest1.Application.Handlers
{
    /// <summary>
    /// UploadFileCommandの処理を実行するハンドラー
    /// </summary>
    public class UploadFileCommandHandler : ICommandHandler<UploadFileCommand>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILogFileHandler _logFileHandler;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileUploadService">ファイルアップロードサービス</param>
        /// <param name="eventDispatcher">イベントディスパッチャー</param>
        /// <param name="logFileHandler">ログファイルハンドラー</param>
        public UploadFileCommandHandler(
            IFileUploadService fileUploadService, 
            IEventDispatcher eventDispatcher,
            ILogFileHandler logFileHandler)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _logFileHandler = logFileHandler ?? throw new ArgumentNullException(nameof(logFileHandler));
        }

        /// <summary>
        /// コマンドの処理を非同期で実行します
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        /// <returns>処理結果</returns>
        public async Task<Result> HandleAsync(UploadFileCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                return Result.Failure("コマンドがnullです。");
            }

            try
            {
                // ファイルアップロードサービスを使用してログファイルをアップロード
                var result = await _fileUploadService.UploadLogFileAsync();

                // イベントの発行
                // 実際にアップロードを試みたファイルパスを使用
                var filePath = _logFileHandler.GetCurrentLogFilePath();

                if (result.IsSuccess)
                {
                    // アップロード成功イベント
                    var blobName = new BlobName($"logs/{DateTime.Now:yyyy/MM/dd}/{filePath.FileName}");
                    // 注意: Application層では実際のストレージアカウント名にアクセスできないため、
                    // Infrastructure層でイベントハンドラーが実際のURIに置き換える想定
                    var blobUri = $"blob://logs/{DateTime.Now:yyyy/MM/dd}/{filePath.FileName}";
                    
                    var uploadedEvent = new FileUploadedEvent(
                        filePath,
                        blobName,
                        blobUri);

                    await _eventDispatcher.DispatchAsync(uploadedEvent, cancellationToken);
                }
                else
                {
                    // アップロード失敗イベント
                    var failedEvent = new FileUploadFailedEvent(
                        filePath,
                        result.ErrorMessage ?? "不明なエラー");

                    await _eventDispatcher.DispatchAsync(failedEvent, cancellationToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                // 予期しない例外をキャッチしてResult.Failureとして返す
                return Result.Failure($"ファイルアップロード中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}