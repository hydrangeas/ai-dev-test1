using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Events;
// using Microsoft.Extensions.Logging; // Application層ではロギングを直接使用しない

namespace AiDevTest1.Application.EventHandlers
{
    /// <summary>
    /// ファイルアップロード失敗イベントのハンドラー
    /// </summary>
    public class FileUploadFailedEventHandler : IEventHandler<FileUploadFailedEvent>
    {
        private readonly IDialogService _dialogService;
        private readonly IRetryPolicy _retryPolicy;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dialogService">ダイアログサービス</param>
        /// <param name="retryPolicy">リトライポリシー</param>
        public FileUploadFailedEventHandler(
            IDialogService dialogService,
            IRetryPolicy retryPolicy)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        }

        /// <summary>
        /// ファイルアップロード失敗イベントを処理します
        /// </summary>
        /// <param name="domainEvent">ドメインイベント</param>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        public Task HandleAsync(FileUploadFailedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            // エラーの種類に応じた処理
            if (IsTransientError(domainEvent.ErrorMessage))
            {
                // 一時的なエラーの場合
                // 将来的にはリトライスケジュールを設定
            }
            else
            {
                // 永続的なエラーの場合
                // 将来的には管理者への通知を検討
            }

            // 将来的な拡張ポイント:
            // - エラー分析とパターン検出
            // - 自動リトライのスケジューリング
            // - エスカレーション通知
            // - 障害レポートの生成

            return Task.CompletedTask;
        }

        /// <summary>
        /// エラーが一時的なものかどうかを判定します
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <returns>一時的なエラーの場合true</returns>
        private bool IsTransientError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return false;

            // 一時的なエラーのパターン
            var transientPatterns = new[]
            {
                "timeout",
                "temporary",
                "unavailable",
                "retry",
                "network",
                "connection"
            };

            var lowerMessage = errorMessage.ToLowerInvariant();
            foreach (var pattern in transientPatterns)
            {
                if (lowerMessage.Contains(pattern))
                    return true;
            }

            return false;
        }
    }
}