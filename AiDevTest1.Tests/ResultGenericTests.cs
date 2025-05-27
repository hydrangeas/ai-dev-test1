using AiDevTest1.Application.Models;
using Xunit;

namespace AiDevTest1.Tests;

/// <summary>
/// Result<T>クラスのユニットテスト
/// </summary>
public class ResultGenericTests
{
  [Fact]
  public void Success_WithValidValue_CreatesSuccessResult()
  {
    // Arrange
    const string expectedValue = "test value";

    // Act
    var result = Result<string>.Success(expectedValue);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.False(result.IsFailure);
    Assert.Equal(expectedValue, result.Value);
    Assert.Null(result.ErrorMessage);
  }

  [Fact]
  public void Success_WithNullValue_ThrowsArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => Result<string?>.Success(null));
  }

  [Fact]
  public void Failure_WithErrorMessage_CreatesFailureResult()
  {
    // Arrange
    const string errorMessage = "Operation failed";

    // Act
    var result = Result<int>.Failure(errorMessage);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailure);
    Assert.Equal(errorMessage, result.ErrorMessage);
  }

  [Fact]
  public void Failure_WithNullErrorMessage_ThrowsArgumentException()
  {
    // Act & Assert
    Assert.Throws<ArgumentException>(() => Result<int>.Failure(null!));
  }

  [Fact]
  public void Failure_WithEmptyErrorMessage_ThrowsArgumentException()
  {
    // Act & Assert
    Assert.Throws<ArgumentException>(() => Result<int>.Failure(string.Empty));
  }

  [Fact]
  public void Failure_WithWhitespaceErrorMessage_ThrowsArgumentException()
  {
    // Act & Assert
    Assert.Throws<ArgumentException>(() => Result<int>.Failure("   "));
  }

  [Fact]
  public void Value_OnFailureResult_ThrowsInvalidOperationException()
  {
    // Arrange
    var result = Result<int>.Failure("Error");

    // Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
    Assert.Contains("Cannot access value of a failed result", exception.Message);
    Assert.Contains("Error", exception.Message);
  }

  [Fact]
  public void ImplicitOperator_WithValue_CreatesSuccessResult()
  {
    // Arrange
    const int expectedValue = 42;

    // Act
    Result<int> result = expectedValue;

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedValue, result.Value);
  }

  [Fact]
  public void Success_WithComplexType_CreatesSuccessResult()
  {
    // Arrange
    var expectedValue = new TestClass { Id = 1, Name = "Test" };

    // Act
    var result = Result<TestClass>.Success(expectedValue);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Same(expectedValue, result.Value);
  }

  [Fact]
  public void Success_WithValueType_CreatesSuccessResult()
  {
    // Arrange
    var expectedValue = new DateTime(2024, 1, 1);

    // Act
    var result = Result<DateTime>.Success(expectedValue);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedValue, result.Value);
  }

  private class TestClass
  {
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
  }
}
