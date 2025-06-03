using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Commands;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;

namespace AiDevTest1.Application.Handlers
{
    /// <summary>
    /// UploadFileCommandの処理を実行するハンドラー
    /// </summary>
    public class UploadFileCommandHandler : ICommandHandler<UploadFileCommand>
    {
        private readonly IFileUploadService _fileUploadService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileUploadService">ファイルアップロードサービス</param>
        public UploadFileCommandHandler(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
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
                return await _fileUploadService.UploadLogFileAsync();
            }
            catch (Exception ex)
            {
                // 予期しない例外をキャッチしてResult.Failureとして返す
                return Result.Failure($"ファイルアップロード中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}