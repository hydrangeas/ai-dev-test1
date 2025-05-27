namespace AiDevTest1.Application.Models;

/// <summary>
/// 操作の成功/失敗と値を表現するResultパターンのジェネリック実装
/// </summary>
/// <typeparam name="T">成功時に返される値の型</typeparam>
public class Result<T> : Result
{
  /// <summary>
  /// 成功時の値
  /// </summary>
  private readonly T? _value;

  /// <summary>
  /// 成功時の値を取得します
  /// </summary>
  /// <exception cref="InvalidOperationException">失敗結果の場合にアクセスするとスローされます</exception>
  public T Value
  {
    get
    {
      if (IsFailure)
      {
        throw new InvalidOperationException($"Cannot access value of a failed result. Error: {ErrorMessage}");
      }
      return _value!;
    }
  }

  /// <summary>
  /// プライベートコンストラクタ（ファクトリメソッドからのみ生成可能）
  /// </summary>
  /// <param name="isSuccess">成功フラグ</param>
  /// <param name="value">成功時の値</param>
  /// <param name="errorMessage">エラーメッセージ</param>
  private Result(bool isSuccess, T? value, string? errorMessage) : base(isSuccess, errorMessage)
  {
    _value = value;
  }

  /// <summary>
  /// 成功結果を作成します
  /// </summary>
  /// <param name="value">成功時の値</param>
  /// <returns>成功と値を表すResultインスタンス</returns>
  public static Result<T> Success(T value)
  {
    if (value == null)
    {
      throw new ArgumentNullException(nameof(value), "Success result must have a value.");
    }
    return new Result<T>(true, value, null);
  }

  /// <summary>
  /// 失敗結果を作成します
  /// </summary>
  /// <param name="errorMessage">エラーメッセージ</param>
  /// <returns>失敗を表すResultインスタンス</returns>
  public new static Result<T> Failure(string errorMessage)
  {
    if (string.IsNullOrWhiteSpace(errorMessage))
    {
      throw new ArgumentException("Error message cannot be null or whitespace.", nameof(errorMessage));
    }
    return new Result<T>(false, default, errorMessage);
  }

  /// <summary>
  /// 暗黙的な型変換演算子（値からResult<T>へ）
  /// </summary>
  /// <param name="value">変換する値</param>
  public static implicit operator Result<T>(T value) => Success(value);
}
