namespace AiDevTest1.Domain.Models;

/// <summary>
/// ログエントリのイベントタイプを表すEnum
/// </summary>
public enum EventType
{
  /// <summary>
  /// アプリケーション開始
  /// </summary>
  START,

  /// <summary>
  /// アプリケーション停止
  /// </summary>
  STOP,

  /// <summary>
  /// 警告
  /// </summary>
  WARN,

  /// <summary>
  /// エラー
  /// </summary>
  ERROR
}
