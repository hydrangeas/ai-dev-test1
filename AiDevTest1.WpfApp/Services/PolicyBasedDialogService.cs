using System;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;

namespace AiDevTest1.WpfApp.Services
{
    /// <summary>
    /// ポリシーベースのダイアログ表示機能の実装クラス
    /// </summary>
    public class PolicyBasedDialogService : IDialogService
    {
        private readonly IDialogService _innerDialogService;
        private readonly IDialogDisplayPolicy _displayPolicy;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="innerDialogService">内部で使用するダイアログサービス</param>
        /// <param name="displayPolicy">ダイアログ表示ポリシー</param>
        public PolicyBasedDialogService(IDialogService innerDialogService, IDialogDisplayPolicy displayPolicy)
        {
            _innerDialogService = innerDialogService ?? throw new ArgumentNullException(nameof(innerDialogService));
            _displayPolicy = displayPolicy ?? throw new ArgumentNullException(nameof(displayPolicy));
        }

        /// <summary>
        /// 成功メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        public void ShowSuccess(string message)
        {
            Task.Run(async () => await _displayPolicy.DisplaySuccessAsync(message, _innerDialogService)).Wait();
        }

        /// <summary>
        /// エラーメッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        public void ShowError(string message)
        {
            Task.Run(async () => await _displayPolicy.DisplayErrorAsync(message, _innerDialogService)).Wait();
        }

        /// <summary>
        /// 警告メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        public void ShowWarning(string message)
        {
            Task.Run(async () => await _displayPolicy.DisplayWarningAsync(message, _innerDialogService)).Wait();
        }

        /// <summary>
        /// 情報メッセージを表示します
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        public void ShowInfo(string message)
        {
            Task.Run(async () => await _displayPolicy.DisplayInfoAsync(message, _innerDialogService)).Wait();
        }
    }
}