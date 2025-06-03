using System.Threading.Tasks;

namespace AiDevTest1.Application.Interfaces
{
    /// <summary>
    /// ダイアログ表示の動作を定義するポリシーインターフェース
    /// </summary>
    public interface IDialogDisplayPolicy
    {
        /// <summary>
        /// 成功メッセージの表示方法を決定します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="dialogService">ダイアログサービス</param>
        Task DisplaySuccessAsync(string message, IDialogService dialogService);

        /// <summary>
        /// エラーメッセージの表示方法を決定します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="dialogService">ダイアログサービス</param>
        Task DisplayErrorAsync(string message, IDialogService dialogService);

        /// <summary>
        /// 警告メッセージの表示方法を決定します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="dialogService">ダイアログサービス</param>
        Task DisplayWarningAsync(string message, IDialogService dialogService);

        /// <summary>
        /// 情報メッセージの表示方法を決定します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="dialogService">ダイアログサービス</param>
        Task DisplayInfoAsync(string message, IDialogService dialogService);
    }
}