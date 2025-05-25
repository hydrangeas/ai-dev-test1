using System;
using System.Globalization;
using System.Windows.Data;

namespace AiDevTest1.WpfApp.Converters
{
  /// <summary>
  /// bool値を反転するコンバーター
  /// IsProcessing プロパティを IsEnabled バインディング用に変換
  /// </summary>
  public class InverseBoolConverter : IValueConverter
  {
    /// <summary>
    /// bool値を反転して返します
    /// </summary>
    /// <param name="value">変換元の値</param>
    /// <param name="targetType">変換先の型</param>
    /// <param name="parameter">変換パラメータ</param>
    /// <param name="culture">カルチャ情報</param>
    /// <returns>反転されたbool値</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is bool boolValue ? !boolValue : Binding.DoNothing;
    }

    /// <summary>
    /// bool値を反転して返します（逆変換）
    /// </summary>
    /// <param name="value">変換元の値</param>
    /// <param name="targetType">変換先の型</param>
    /// <param name="parameter">変換パラメータ</param>
    /// <param name="culture">カルチャ情報</param>
    /// <returns>反転されたbool値</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is bool boolValue ? !boolValue : Binding.DoNothing;
    }
  }
}
