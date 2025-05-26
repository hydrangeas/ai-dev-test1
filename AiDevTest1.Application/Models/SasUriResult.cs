namespace AiDevTest1.Application.Models;

/// <summary>
/// SAS URI取得操作の結果を表現するクラス
/// </summary>
public class SasUriResult
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
  /// 取得されたSAS URI
  /// </summary>
  public string? SasUri { get; }

  /// <summary>
  /// 相関ID（アップロード完了通知で使用）
  /// </summary>
  public string? CorrelationId { get; }

  /// <summary>
  /// プライベートコンストラクタ
  /// </summary>
  /// <param name="result">基本結果</param>
  /// <param name="sasUri">SAS URI</param>
  /// <param name="correlationId">相関ID</param>
  private SasUriResult(Result result, string? sasUri, string? correlationId)
  {
    Result = result;
    SasUri = sasUri;
    CorrelationId = correlationId;
  }

  /// <summary>
  /// 成功結果を作成します
  /// </summary>
  /// <param name="sasUri">SAS URI</param>
  /// <param name="correlationId">相関ID</param>
  /// <returns>成功を表すSasUriResultインスタンス</returns>
  public static SasUriResult Success(string sasUri, string correlationId)
      => new(Result.Success(), sasUri, correlationId);

  /// <summary>
  /// 失敗結果を作成します
  /// </summary>
  /// <param name="errorMessage">エラーメッセージ</param>
  /// <returns>失敗を表すSasUriResultインスタンス</returns>
  public static SasUriResult Failure(string errorMessage)
      => new(Result.Failure(errorMessage), null, null);
}
