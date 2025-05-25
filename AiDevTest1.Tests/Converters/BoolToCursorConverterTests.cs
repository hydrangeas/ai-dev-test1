using AiDevTest1.WpfApp.Converters;
using System;
using System.Globalization;
using System.Windows.Input;
using Xunit;

namespace AiDevTest1.Tests.Converters
{
  /// <summary>
  /// BoolToCursorConverterのテストクラス
  /// </summary>
  public class BoolToCursorConverterTests
  {
    private readonly BoolToCursorConverter _converter;
    private readonly CultureInfo _culture;

    public BoolToCursorConverterTests()
    {
      _converter = new BoolToCursorConverter();
      _culture = CultureInfo.InvariantCulture;
    }

    [Fact]
    public void Convert_WithTrueValue_ReturnsWaitCursor()
    {
      // Arrange
      var input = true;

      // Act
      var result = _converter.Convert(input, typeof(Cursor), null!, _culture);

      // Assert
      Assert.Equal(Cursors.Wait, result);
    }

    [Fact]
    public void Convert_WithFalseValue_ReturnsArrowCursor()
    {
      // Arrange
      var input = false;

      // Act
      var result = _converter.Convert(input, typeof(Cursor), null!, _culture);

      // Assert
      Assert.Equal(Cursors.Arrow, result);
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsArrowCursor()
    {
      // Arrange
      object? input = null;

      // Act
      var result = _converter.Convert(input!, typeof(Cursor), null!, _culture);

      // Assert
      Assert.Equal(Cursors.Arrow, result);
    }

    [Fact]
    public void Convert_WithNonBoolValue_ReturnsArrowCursor()
    {
      // Arrange
      var input = "not a bool";

      // Act
      var result = _converter.Convert(input, typeof(Cursor), null!, _culture);

      // Assert
      Assert.Equal(Cursors.Arrow, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
      // Arrange
      var input = Cursors.Wait;

      // Act & Assert
      Assert.Throws<NotImplementedException>(() =>
        _converter.ConvertBack(input, typeof(bool), null!, _culture));
    }
  }
}
