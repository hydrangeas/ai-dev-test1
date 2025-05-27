using System.Windows;

namespace AiDevTest1.WpfApp.Helpers
{
  /// <summary>
  /// ダイアログ表示のためのヘルパークラス
  /// </summary>
  public static class DialogHelper
  {
    /// <summary>
    /// 成功ダイアログを表示します
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    public static void ShowSuccess(string message)
    {
      MessageBox.Show(message, "成功", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 失敗ダイアログを表示します
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    public static void ShowFailure(string message)
    {
      MessageBox.Show(message, "失敗", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// 警告ダイアログを表示します
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    public static void ShowWarning(string message)
    {
      MessageBox.Show(message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
  }
}
