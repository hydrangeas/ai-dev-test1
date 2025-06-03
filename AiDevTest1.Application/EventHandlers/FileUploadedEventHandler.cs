using System;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Events;
// using Microsoft.Extensions.Logging; // Application層ではロギングを直接使用しない

namespace AiDevTest1.Application.EventHandlers
{
    /// <summary>
    /// ファイルアップロード成功イベントのハンドラー
    /// </summary>
    public class FileUploadedEventHandler : IEventHandler<FileUploadedEvent>
    {
        private readonly IDialogService _dialogService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dialogService">ダイアログサービス</param>
        public FileUploadedEventHandler(IDialogService dialogService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        /// <summary>
        /// ファイルアップロード成功イベントを処理します
        /// </summary>
        /// <param name="domainEvent">ドメインイベント</param>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        public Task HandleAsync(FileUploadedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            // アップロード成功の処理
            // 将来的には成功通知を表示する可能性がある
            // 現在は特に処理を行わない

            // 将来的な拡張ポイント:
            // - アップロード成功回数の統計
            // - 次回アップロードスケジュールの調整
            // - 成功通知の送信
            // - ダッシュボードの更新

            return Task.CompletedTask;
        }
    }
}