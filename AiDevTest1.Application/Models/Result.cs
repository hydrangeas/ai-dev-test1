namespace AiDevTest1.Application.Models;

/// <summary>
/// 操作の成功/失敗を表現するResultパターンの実装
/// </summary>
public class Result
{
  /// <summary>
  /// 操作が成功したかどうかを示します
  /// </summary>
  public bool IsSuccess { get; }

  /// <summary>
  /// 操作が失敗したかどうかを示します
  /// </summary>
  public bool IsFailure => !IsSuccess;

  /// <summary>
  /// エラーメッセージ（失敗時のみ設定）
  /// </summary>
  public string? ErrorMessage { get; }

  /// <summary>
  /// プライベートコンストラクタ（ファクトリメソッドからのみ生成可能）
  /// </summary>
  /// <param name="isSuccess">成功フラグ</param>
  /// <param name="errorMessage">エラーメッセージ</param>
  private Result(bool isSuccess, string? errorMessage)
  {
    IsSuccess = isSuccess;
    ErrorMessage = errorMessage;
  }

  /// <summary>
  /// 成功結果を作成します
  /// </summary>
  /// <returns>成功を表すResultインスタンス</returns>
  public static Result Success() => new(true, null);

  /// <summary>
  /// 失敗結果を作成します
  /// </summary>
  /// <param name="errorMessage">エラーメッセージ</param>
  /// <returns>失敗を表すResultインスタンス</returns>
  public static Result Failure(string errorMessage) => new(false, errorMessage);
}
