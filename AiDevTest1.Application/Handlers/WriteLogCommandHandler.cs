using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Commands;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;

namespace AiDevTest1.Application.Handlers
{
    /// <summary>
    /// WriteLogCommandの処理を実行するハンドラー
    /// </summary>
    public class WriteLogCommandHandler : ICommandHandler<WriteLogCommand>
    {
        private readonly ILogWriteService _logWriteService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logWriteService">ログ書き込みサービス</param>
        public WriteLogCommandHandler(ILogWriteService logWriteService)
        {
            _logWriteService = logWriteService ?? throw new ArgumentNullException(nameof(logWriteService));
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
                return await _logWriteService.WriteLogEntryAsync();
            }
            catch (Exception ex)
            {
                // 予期しない例外をキャッチしてResult.Failureとして返す
                return Result.Failure($"ログの書き込み中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}