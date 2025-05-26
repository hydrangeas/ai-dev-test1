namespace AiDevTest1.Application.Models;

/// <summary>
/// Blobアップロード操作の結果を表現するクラス
/// </summary>
public class UploadToBlobResult
{
  /// <summary>
  /// 基本的な結果情報
  /// </summary>
  public Result Result { get; }

  /// <summary>
  /// 操作が成功したかどうかを示します
  /// </summary>
  public bool IsSuccess => Result.IsSuccess;

  /// <summary>
  /// 操作が失敗したかどうかを示します
  /// </summary>
  public bool IsFailure => Result.IsFailure;

  /// <summary>
  /// エラーメッセージ（失敗時のみ設定）
  /// </summary>
  public string? ErrorMessage => Result.ErrorMessage;

  /// <summary>
  /// プライベートコンストラクタ
  /// </summary>
  /// <param name="result">基本結果</param>
  private UploadToBlobResult(Result result)
  {
    Result = result;
  }

  /// <summary>
  /// 成功結果を作成します
  /// </summary>
  /// <returns>成功を表すUploadToBlobResultインスタンス</returns>
  public static UploadToBlobResult Success()
      => new(Result.Success());

  /// <summary>
  /// 失敗結果を作成します
  /// </summary>
  /// <param name="errorMessage">エラーメッセージ</param>
  /// <returns>失敗を表すUploadToBlobResultインスタンス</returns>
  public static UploadToBlobResult Failure(string errorMessage)
      => new(Result.Failure(errorMessage));
}
