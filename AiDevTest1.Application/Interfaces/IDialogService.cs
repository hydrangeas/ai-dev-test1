using System.Threading.Tasks;

namespace AiDevTest1.Application.Interfaces
{
    /// <summary>
    /// ダイアログ表示機能を提供するインターフェース
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 成功メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        void ShowSuccess(string message);

        /// <summary>
        /// エラーメッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        void ShowError(string message);

        /// <summary>
        /// 警告メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        void ShowWarning(string message);

        /// <summary>
        /// 情報メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        void ShowInfo(string message);
    }
}