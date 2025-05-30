using AiDevTest1.Domain.Exceptions;
using System;
using Xunit;

namespace AiDevTest1.Tests
{
  /// <summary>
  /// DomainExceptionクラスのユニットテスト
  /// </summary>
  public class DomainExceptionTests
  {
    [Fact]
    public void Constructor_WithMessage_SetsPropertiesCorrectly()
    {
      // Arrange
      const string message = "Test domain exception";

      // Act
      var exception = new DomainException(message);

      // Assert
      Assert.Equal(message, exception.Message);
      Assert.Equal("Domain", exception.ErrorCode);
      Assert.Equal("Domain", exception.Category);
    }

    [Fact]
    public void Constructor_WithMessageAndErrorCode_SetsPropertiesCorrectly()
    {
      // Arrange
      const string message = "Test domain exception";
      const string errorCode = "TEST_ERROR";

      // Act
      var exception = new DomainException(message, errorCode);

      // Assert
      Assert.Equal(message, exception.Message);
      Assert.Equal(errorCode, exception.ErrorCode);
      Assert.Equal("Domain", exception.Category);
    }

    [Fact]
    public void Constructor_WithMessageErrorCodeAndCategory_SetsPropertiesCorrectly()
    {
      // Arrange
      const string message = "Test domain exception";
      const string errorCode = "TEST_ERROR";
      const string category = "Validation";

      // Act
      var exception = new DomainException(message, errorCode, category);

      // Assert
      Assert.Equal(message, exception.Message);
      Assert.Equal(errorCode, exception.ErrorCode);
      Assert.Equal(category, exception.Category);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsPropertiesCorrectly()
    {
      // Arrange
      const string message = "Test domain exception";
      var innerException = new InvalidOperationException("Inner exception");

      // Act
      var exception = new DomainException(message, innerException);

      // Assert
      Assert.Equal(message, exception.Message);
      Assert.Equal("Domain", exception.ErrorCode);
      Assert.Equal("Domain", exception.Category);
      Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageErrorCodeAndInnerException_SetsPropertiesCorrectly()
    {
      // Arrange
      const string message = "Test domain exception";
      const string errorCode = "TEST_ERROR";
      var innerException = new InvalidOperationException("Inner exception");

      // Act
      var exception = new DomainException(message, errorCode, innerException);

      // Assert
      Assert.Equal(message, exception.Message);
      Assert.Equal(errorCode, exception.ErrorCode);
      Assert.Equal("Domain", exception.Category);
      Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsPropertiesCorrectly()
    {
      // Arrange
      const string message = "Test domain exception";
      const string errorCode = "TEST_ERROR";
      const string category = "Validation";
      var innerException = new InvalidOperationException("Inner exception");

      // Act
      var exception = new DomainException(message, errorCode, category, innerException);

      // Assert
      Assert.Equal(message, exception.Message);
      Assert.Equal(errorCode, exception.ErrorCode);
      Assert.Equal(category, exception.Category);
      Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_WithNullErrorCode_UsesDefaultErrorCode()
    {
      // Arrange
      const string message = "Test domain exception";

      // Act
      var exception = new DomainException(message, (string?)null);

      // Assert
      Assert.Equal("Domain", exception.ErrorCode);
    }

    [Fact]
    public void Constructor_WithNullCategory_UsesDefaultCategory()
    {
      // Arrange
      const string message = "Test domain exception";
      const string errorCode = "TEST_ERROR";

      // Act
      var exception = new DomainException(message, errorCode, (string?)null);

      // Assert
      Assert.Equal("Domain", exception.Category);
    }

    [Fact]
    public void DerivedClass_AutomaticallyGeneratesErrorCodeFromClassName()
    {
      // Act
      var exception = new TestDomainException("Test message");

      // Assert
      Assert.Equal("TestDomain", exception.ErrorCode);
    }

    [Fact]
    public void DomainException_IsTypeOfException()
    {
      // Arrange
      var exception = new DomainException("Test");

      // Assert
      Assert.IsAssignableFrom<Exception>(exception);
    }

    // 派生クラスのテスト用
    private class TestDomainException : DomainException
    {
      public TestDomainException(string message) : base(message) { }
    }
  }
}
