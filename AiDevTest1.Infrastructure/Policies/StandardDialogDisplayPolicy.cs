using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;

namespace AiDevTest1.Infrastructure.Policies
{
    /// <summary>
    /// 標準的なダイアログ表示ポリシーの実装
    /// </summary>
    public class StandardDialogDisplayPolicy : IDialogDisplayPolicy
    {
        /// <summary>
        /// 成功メッセージを標準的な方法で表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="dialogService">ダイアログサービス</param>
        public Task DisplaySuccessAsync(string message, IDialogService dialogService)
        {
            dialogService.ShowSuccess(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// エラーメッセージを標準的な方法で表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="dialogService">ダイアログサービス</param>
        public Task DisplayErrorAsync(string message, IDialogService dialogService)
        {
            dialogService.ShowError(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 警告メッセージを標準的な方法で表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="dialogService">ダイアログサービス</param>
        public Task DisplayWarningAsync(string message, IDialogService dialogService)
        {
            dialogService.ShowWarning(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 情報メッセージを標準的な方法で表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="dialogService">ダイアログサービス</param>
        public Task DisplayInfoAsync(string message, IDialogService dialogService)
        {
            dialogService.ShowInfo(message);
            return Task.CompletedTask;
        }
    }
}