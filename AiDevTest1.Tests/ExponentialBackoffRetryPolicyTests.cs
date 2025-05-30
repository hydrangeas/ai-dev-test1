using AiDevTest1.Infrastructure.Policies;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AiDevTest1.Tests
{
  public class ExponentialBackoffRetryPolicyTests
  {
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
      // Arrange & Act
      var retryPolicy = new ExponentialBackoffRetryPolicy(maxRetryCount: 5, initialDelayMilliseconds: 500);

      // Assert
      Assert.NotNull(retryPolicy);
    }

    [Fact]
    public void Constructor_WithNegativeMaxRetryCount_ShouldThrowArgumentOutOfRangeException()
    {
      // Arrange & Act & Assert
      var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
          new ExponentialBackoffRetryPolicy(maxRetryCount: -1));

      Assert.Equal("maxRetryCount", exception.ParamName);
      Assert.Contains("最大リトライ回数は0以上である必要があります", exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativeInitialDelay_ShouldThrowArgumentOutOfRangeException()
    {
      // Arrange & Act & Assert
      var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
          new ExponentialBackoffRetryPolicy(initialDelayMilliseconds: -1));

      Assert.Equal("initialDelayMilliseconds", exception.ParamName);
      Assert.Contains("初期遅延時間は0以上である必要があります", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy();
      const string expectedResult = "success";

      // Act
      var result = await retryPolicy.ExecuteAsync(async () =>
      {
        await Task.Delay(10);
        return expectedResult;
      });

      // Assert
      Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullOperation_ShouldThrowArgumentNullException()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy();

      // Act & Assert
      var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
          retryPolicy.ExecuteAsync<string>(null!));

      Assert.Equal("operation", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingOperationAndNoRetryableExceptions_ShouldRetryAndThrowAggregateException()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy(maxRetryCount: 2, initialDelayMilliseconds: 10);
      var attemptCount = 0;

      // Act & Assert
      var aggregateException = await Assert.ThrowsAsync<AggregateException>(() =>
          retryPolicy.ExecuteAsync(async () =>
          {
            attemptCount++;
            await Task.Delay(1);
            throw new InvalidOperationException($"Failed attempt {attemptCount}");
          }));

      // Assert
      Assert.Equal(3, attemptCount); // 初回 + 2回のリトライ
      Assert.Equal(3, aggregateException.InnerExceptions.Count);
      Assert.All(aggregateException.InnerExceptions, ex => Assert.IsType<InvalidOperationException>(ex));
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingOperationThenSuccess_ShouldReturnSuccessResult()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy(maxRetryCount: 2, initialDelayMilliseconds: 10);
      var attemptCount = 0;
      const string expectedResult = "success after retry";

      // Act
      var result = await retryPolicy.ExecuteAsync(async () =>
      {
        attemptCount++;
        await Task.Delay(1);

        if (attemptCount <= 2)
        {
          throw new InvalidOperationException($"Failed attempt {attemptCount}");
        }

        return expectedResult;
      });

      // Assert
      Assert.Equal(3, attemptCount);
      Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldRespectCancellation()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy(maxRetryCount: 5, initialDelayMilliseconds: 100);
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(50); // 50ms後にキャンセル

      // Act & Assert
      await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
          retryPolicy.ExecuteAsync(async () =>
          {
            await Task.Delay(200, cts.Token); // 200msの遅延（キャンセルより長い）
            return "should not reach here";
          }, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_VoidOperation_WithSuccessfulOperation_ShouldComplete()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy();
      var executed = false;

      // Act
      await retryPolicy.ExecuteAsync(async () =>
      {
        await Task.Delay(10);
        executed = true;
      });

      // Assert
      Assert.True(executed);
    }

    [Fact]
    public async Task ExecuteAsync_VoidOperation_WithNullOperation_ShouldThrowArgumentNullException()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy();

      // Act & Assert
      var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
          retryPolicy.ExecuteAsync((Func<Task>)null!));

      Assert.Equal("operation", exception.ParamName);
    }

    [Fact]
    public void AddRetryableException_Generic_ShouldAddExceptionType()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy();

      // Act
      var result = retryPolicy.AddRetryableException<InvalidOperationException>();

      // Assert
      Assert.Same(retryPolicy, result); // メソッドチェーンのテスト
    }

    [Fact]
    public void AddRetryableException_WithType_ShouldAddExceptionType()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy();

      // Act
      var result = retryPolicy.AddRetryableException(typeof(ArgumentException));

      // Assert
      Assert.Same(retryPolicy, result); // メソッドチェーンのテスト
    }

    [Fact]
    public void AddRetryableException_WithNullType_ShouldThrowArgumentNullException()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy();

      // Act & Assert
      var exception = Assert.Throws<ArgumentNullException>(() =>
          retryPolicy.AddRetryableException(null!));

      Assert.Equal("exceptionType", exception.ParamName);
    }

    [Fact]
    public void AddRetryableException_WithNonExceptionType_ShouldThrowArgumentException()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy();

      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() =>
          retryPolicy.AddRetryableException(typeof(string)));

      Assert.Equal("exceptionType", exception.ParamName);
      Assert.Contains("指定された型はExceptionを継承している必要があります", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithSpecificRetryableExceptions_ShouldOnlyRetrySpecifiedExceptions()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy(maxRetryCount: 2, initialDelayMilliseconds: 10)
          .AddRetryableException<InvalidOperationException>();

      var attemptCount = 0;

      // Act & Assert
      var aggregateException = await Assert.ThrowsAsync<AggregateException>(() =>
          retryPolicy.ExecuteAsync(async () =>
          {
            attemptCount++;
            await Task.Delay(1);

            if (attemptCount == 1)
            {
              throw new ArgumentException("First failure - should not retry");
            }

            return "should not reach here";
          }));

      // Assert
      Assert.Equal(1, attemptCount); // リトライ対象外なので1回のみ実行
      Assert.Single(aggregateException.InnerExceptions);
      Assert.IsType<ArgumentException>(aggregateException.InnerExceptions[0]);
    }

    [Fact]
    public async Task ExecuteAsync_WithInheritedRetryableExceptions_ShouldRetryInheritedTypes()
    {
      // Arrange
      var retryPolicy = new ExponentialBackoffRetryPolicy(maxRetryCount: 2, initialDelayMilliseconds: 10)
          .AddRetryableException<SystemException>(); // InvalidOperationExceptionの基底クラス

      var attemptCount = 0;

      // Act & Assert
      var aggregateException = await Assert.ThrowsAsync<AggregateException>(() =>
          retryPolicy.ExecuteAsync(async () =>
          {
            attemptCount++;
            await Task.Delay(1);
            throw new InvalidOperationException($"Failed attempt {attemptCount}");
          }));

      // Assert
      Assert.Equal(3, attemptCount); // 初回 + 2回のリトライ
      Assert.Equal(3, aggregateException.InnerExceptions.Count);
    }
  }
}
