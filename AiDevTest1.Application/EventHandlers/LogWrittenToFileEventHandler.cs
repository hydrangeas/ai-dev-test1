using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Events;
// using Microsoft.Extensions.Logging; // Application層ではロギングを直接使用しない

namespace AiDevTest1.Application.EventHandlers
{
    /// <summary>
    /// ログファイル書き込みイベントのハンドラー
    /// </summary>
    public class LogWrittenToFileEventHandler : IEventHandler<LogWrittenToFileEvent>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LogWrittenToFileEventHandler()
        {
        }

        /// <summary>
        /// ログ書き込みイベントを処理します
        /// </summary>
        /// <param name="domainEvent">ドメインイベント</param>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        public Task HandleAsync(LogWrittenToFileEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            // イベント情報の処理
            // 実際のロギングはInfrastructure層で行う
            // ここではドメインロジックに集中

            // 将来的な拡張ポイント:
            // - メトリクスの収集
            // - 監査ログの記録
            // - 通知の送信
            // - 統計情報の更新

            return Task.CompletedTask;
        }
    }
}