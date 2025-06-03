using AiDevTest1.Application.Interfaces;
using AiDevTest1.WpfApp.Helpers;

namespace AiDevTest1.WpfApp.Services
{
    /// <summary>
    /// ダイアログ表示機能の実装クラス
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// 成功メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        public void ShowSuccess(string message)
        {
            DialogHelper.ShowSuccess(message);
        }

        /// <summary>
        /// エラーメッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        public void ShowError(string message)
        {
            DialogHelper.ShowFailure(message);
        }

        /// <summary>
        /// 警告メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        public void ShowWarning(string message)
        {
            DialogHelper.ShowWarning(message);
        }

        /// <summary>
        /// 情報メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        public void ShowInfo(string message)
        {
            // DialogHelperには情報メッセージ用のメソッドがないため、成功メッセージで代用
            DialogHelper.ShowSuccess(message);
        }
    }
}