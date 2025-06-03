using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Commands;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;
using AiDevTest1.Domain.Events;

namespace AiDevTest1.Application.Handlers
{
    /// <summary>
    /// WriteLogCommandの処理を実行するハンドラー
    /// </summary>
    public class WriteLogCommandHandler : ICommandHandler<WriteLogCommand>
    {
        private readonly ILogWriteService _logWriteService;
        private readonly IEventDispatcher _eventDispatcher;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logWriteService">ログ書き込みサービス</param>
        /// <param name="eventDispatcher">イベントディスパッチャー</param>
        public WriteLogCommandHandler(ILogWriteService logWriteService, IEventDispatcher eventDispatcher)
        {
            _logWriteService = logWriteService ?? throw new ArgumentNullException(nameof(logWriteService));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        /// <summary>
        /// コマンドの処理を非同期で実行します
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        /// <returns>処理結果</returns>
        public async Task<Result> HandleAsync(WriteLogCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                return Result.Failure("コマンドがnullです。");
            }

            try
            {
                // ログ書き込みサービスを使用してログエントリを書き込む
                var result = await _logWriteService.WriteLogEntryAsync();

                if (result.IsSuccess)
                {
                    // 成功時はイベントを発行
                    // 注意: 現在の実装ではLogFile集約が使用されていないため、
                    // ここで擬似的にイベントを作成して配信します
                    var logWrittenEvent = new LogWrittenToFileEvent(
                        new Domain.ValueObjects.LogFilePath($"{DateTime.Now:yyyy-MM-dd}.log"),
                        new Domain.Models.LogEntry(
                            Domain.Models.EventType.START,
                            "Application started",
                            DateTime.Now));

                    await _eventDispatcher.DispatchAsync(logWrittenEvent, cancellationToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                // 予期しない例外をキャッチしてResult.Failureとして返す
                return Result.Failure($"ログの書き込み中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}