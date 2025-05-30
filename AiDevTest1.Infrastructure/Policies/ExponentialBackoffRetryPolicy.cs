using AiDevTest1.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiDevTest1.Infrastructure.Policies
{
  /// <summary>
  /// 指数バックオフを使用したリトライポリシー
  /// リトライ間隔を指数的に増加させながら失敗した操作を再実行します
  /// </summary>
  public class ExponentialBackoffRetryPolicy : IRetryPolicy
  {
    private readonly int _maxRetryCount;
    private readonly TimeSpan _initialDelay;
    private readonly HashSet<Type> _retryableExceptionTypes;

    /// <summary>
    /// ExponentialBackoffRetryPolicyの新しいインスタンスを初期化します
    /// </summary>
    /// <param name="maxRetryCount">最大リトライ回数（デフォルト: 3）</param>
    /// <param name="initialDelayMilliseconds">初期遅延時間（ミリ秒、デフォルト: 1000）</param>
    /// <exception cref="ArgumentOutOfRangeException">maxRetryCountまたはinitialDelayMillisecondsが負の値の場合</exception>
    public ExponentialBackoffRetryPolicy(
        int maxRetryCount = 3,
        int initialDelayMilliseconds = 1000)
    {
      if (maxRetryCount < 0)
        throw new ArgumentOutOfRangeException(nameof(maxRetryCount), "最大リトライ回数は0以上である必要があります");
      if (initialDelayMilliseconds < 0)
        throw new ArgumentOutOfRangeException(nameof(initialDelayMilliseconds), "初期遅延時間は0以上である必要があります");

      _maxRetryCount = maxRetryCount;
      _initialDelay = TimeSpan.FromMilliseconds(initialDelayMilliseconds);
      _retryableExceptionTypes = new HashSet<Type>();
    }

    /// <summary>
    /// リトライ対象の例外タイプを追加します
    /// </summary>
    /// <param name="exceptionType">リトライ対象とする例外タイプ</param>
    /// <returns>メソッドチェーン用の自身のインスタンス</returns>
    /// <exception cref="ArgumentNullException">exceptionTypeがnullの場合</exception>
    public ExponentialBackoffRetryPolicy AddRetryableException<T>() where T : Exception
    {
      _retryableExceptionTypes.Add(typeof(T));
      return this;
    }

    /// <summary>
    /// リトライ対象の例外タイプを追加します
    /// </summary>
    /// <param name="exceptionType">リトライ対象とする例外タイプ</param>
    /// <returns>メソッドチェーン用の自身のインスタンス</returns>
    /// <exception cref="ArgumentNullException">exceptionTypeがnullの場合</exception>
    public ExponentialBackoffRetryPolicy AddRetryableException(Type exceptionType)
    {
      if (exceptionType == null)
        throw new ArgumentNullException(nameof(exceptionType));
      if (!typeof(Exception).IsAssignableFrom(exceptionType))
        throw new ArgumentException("指定された型はExceptionを継承している必要があります", nameof(exceptionType));

      _retryableExceptionTypes.Add(exceptionType);
      return this;
    }

    /// <summary>
    /// 指定された操作をリトライポリシーに従って実行します
    /// </summary>
    /// <typeparam name="T">操作の戻り値の型</typeparam>
    /// <param name="operation">実行する非同期操作</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>操作の実行結果</returns>
    /// <exception cref="ArgumentNullException">operationがnullの場合</exception>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
      if (operation == null)
        throw new ArgumentNullException(nameof(operation));

      var attemptErrors = new List<Exception>();

      for (int attempt = 1; attempt <= _maxRetryCount + 1; attempt++)
      {
        try
        {
          cancellationToken.ThrowIfCancellationRequested();
          return await operation();
        }
        catch (Exception ex) when (attempt <= _maxRetryCount && ShouldRetry(ex))
        {
          attemptErrors.Add(ex);

          if (attempt <= _maxRetryCount)
          {
            var delay = CalculateDelay(attempt);
            await Task.Delay(delay, cancellationToken);
          }
        }
        catch (Exception ex)
        {
          // リトライ対象外の例外、または最後の試行での例外
          attemptErrors.Add(ex);
          throw new AggregateException(
            $"操作が{attempt}回の試行で失敗しました。",
            attemptErrors);
        }
      }

      // この行に到達することはないはずですが、コンパイラエラーを回避するため
      throw new AggregateException(
        $"操作が{_maxRetryCount + 1}回の試行すべてで失敗しました。",
        attemptErrors);
    }

    /// <summary>
    /// 指定された操作をリトライポリシーに従って実行します（戻り値なし）
    /// </summary>
    /// <param name="operation">実行する非同期操作</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <exception cref="ArgumentNullException">operationがnullの場合</exception>
    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
      if (operation == null)
        throw new ArgumentNullException(nameof(operation));

      await ExecuteAsync(async () =>
      {
        await operation();
        return true; // ダミーの戻り値
      }, cancellationToken);
    }

    /// <summary>
    /// 例外がリトライ対象かどうかを判定します
    /// </summary>
    /// <param name="exception">判定対象の例外</param>
    /// <returns>リトライ対象の場合はtrue、それ以外はfalse</returns>
    private bool ShouldRetry(Exception exception)
    {
      // リトライ対象例外が指定されていない場合は、すべての例外をリトライ対象とする
      if (_retryableExceptionTypes.Count == 0)
        return true;

      // 指定された例外タイプのいずれかに該当するかチェック
      foreach (var retryableType in _retryableExceptionTypes)
      {
        if (retryableType.IsAssignableFrom(exception.GetType()))
          return true;
      }

      return false;
    }

    /// <summary>
    /// 指数バックオフに基づく遅延時間を計算します
    /// </summary>
    /// <param name="attempt">試行回数（1から開始）</param>
    /// <returns>遅延時間</returns>
    private TimeSpan CalculateDelay(int attempt)
    {
      // 指数バックオフ: initialDelay * 2^(attempt-1)
      var multiplier = Math.Pow(2, attempt - 1);
      var delayMilliseconds = _initialDelay.TotalMilliseconds * multiplier;

      // 最大遅延時間を30秒に制限
      const int maxDelayMs = 30000;
      delayMilliseconds = Math.Min(delayMilliseconds, maxDelayMs);

      return TimeSpan.FromMilliseconds(delayMilliseconds);
    }
  }
}
