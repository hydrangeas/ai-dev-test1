using AiDevTest1.Domain.Models;

namespace AiDevTest1.Domain.Interfaces;

/// <summary>
/// LogEntryオブジェクトを生成するファクトリのインターフェース
/// </summary>
public interface ILogEntryFactory
{
  /// <summary>
  /// 指定されたEventTypeに基づいてLogEntryオブジェクトを作成します
  /// </summary>
  /// <param name="eventType">作成するログエントリのイベントタイプ</param>
  /// <returns>生成されたLogEntryオブジェクト</returns>
  LogEntry CreateLogEntry(EventType eventType);

  /// <summary>
  /// ランダムなEventTypeとメッセージでLogEntryオブジェクトを作成します
  /// </summary>
  /// <returns>生成されたLogEntryオブジェクト</returns>
  LogEntry CreateLogEntry();
}
