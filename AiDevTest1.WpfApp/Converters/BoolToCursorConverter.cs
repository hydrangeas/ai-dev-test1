using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace AiDevTest1.WpfApp.Converters
{
  /// <summary>
  /// bool値をCursorに変換するコンバーター
  /// IsProcessing プロパティを待機カーソル/通常カーソルに変換
  /// </summary>
  public class BoolToCursorConverter : IValueConverter
  {
    /// <summary>
    /// bool値をCursorに変換します
    /// </summary>
    /// <param name="value">変換元の値</param>
    /// <param name="targetType">変換先の型</param>
    /// <param name="parameter">変換パラメータ</param>
    /// <param name="culture">カルチャ情報</param>
    /// <returns>待機中の場合はWaitカーソル、そうでなければArrowカーソル</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is bool isProcessing && isProcessing ? Cursors.Wait : Cursors.Arrow;
    }

    /// <summary>
    /// Cursorからbool値への逆変換（実装しない）
    /// </summary>
    /// <param name="value">変換元の値</param>
    /// <param name="targetType">変換先の型</param>
    /// <param name="parameter">変換パラメータ</param>
    /// <param name="culture">カルチャ情報</param>
    /// <returns>常にNotImplementedExceptionをスロー</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException("BoolToCursorConverter does not support ConvertBack.");
    }
  }
}
