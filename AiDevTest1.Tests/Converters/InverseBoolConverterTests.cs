using AiDevTest1.WpfApp.Converters;
using System;
using System.Globalization;
using System.Windows.Data;
using Xunit;

namespace AiDevTest1.Tests.Converters
{
  /// <summary>
  /// InverseBoolConverterのテストクラス
  /// </summary>
  public class InverseBoolConverterTests
  {
    private readonly InverseBoolConverter _converter;
    private readonly CultureInfo _culture;

    public InverseBoolConverterTests()
    {
      _converter = new InverseBoolConverter();
      _culture = CultureInfo.InvariantCulture;
    }

    [Fact]
    public void Convert_WithTrueValue_ReturnsFalse()
    {
      // Arrange
      var input = true;

      // Act
      var result = _converter.Convert(input, typeof(bool), null, _culture);

      // Assert
      Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_WithFalseValue_ReturnsTrue()
    {
      // Arrange
      var input = false;

      // Act
      var result = _converter.Convert(input, typeof(bool), null, _culture);

      // Assert
      Assert.Equal(true, result);
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsDoNothing()
    {
      // Arrange
      object input = null;

      // Act
      var result = _converter.Convert(input, typeof(bool), null, _culture);

      // Assert
      Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void Convert_WithNonBoolValue_ReturnsDoNothing()
    {
      // Arrange
      var input = "not a bool";

      // Act
      var result = _converter.Convert(input, typeof(bool), null, _culture);

      // Assert
      Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBack_WithTrueValue_ReturnsFalse()
    {
      // Arrange
      var input = true;

      // Act
      var result = _converter.ConvertBack(input, typeof(bool), null, _culture);

      // Assert
      Assert.Equal(false, result);
    }

    [Fact]
    public void ConvertBack_WithFalseValue_ReturnsTrue()
    {
      // Arrange
      var input = false;

      // Act
      var result = _converter.ConvertBack(input, typeof(bool), null, _culture);

      // Assert
      Assert.Equal(true, result);
    }
  }
}
