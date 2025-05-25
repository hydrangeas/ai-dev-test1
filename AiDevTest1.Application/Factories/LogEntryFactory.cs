using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Models;

namespace AiDevTest1.Application.Factories;

/// <summary>
/// LogEntryオブジェクトを生成するファクトリクラス
/// </summary>
public class LogEntryFactory : ILogEntryFactory
{
  /// <summary>
  /// ランダム選択用のRandomインスタンス
  /// </summary>
  private static readonly Random Random = new();

  /// <summary>
  /// EventTypeごとのメッセージ定義
  /// </summary>
  private static readonly Dictionary<EventType, string[]> EventMessages = new()
  {
    {
      EventType.START,
      new[]
      {
        "運転を開始しました。",
        "加工シーケンスを開始します。"
      }
    },
    {
      EventType.STOP,
      new[]
      {
        "運転を停止しました。",
        "現在の加工サイクルを完了し、停止しました。"
      }
    },
    {
      EventType.WARN,
      new[]
      {
        "主軸モーターの温度が上昇しています。確認してください。",
        "切削油の残量が少なくなっています。補充を検討してください。"
      }
    },
    {
      EventType.ERROR,
      new[]
      {
        "サーボモーターエラーが発生しました。システムを停止します。 (コード: E012)",
        "工具が破損しました。交換が必要です。機械を停止しました。"
      }
    }
  };

  /// <summary>
  /// 指定されたEventTypeに基づいてLogEntryオブジェクトを作成します
  /// </summary>
  /// <param name="eventType">作成するログエントリのイベントタイプ</param>
  /// <returns>生成されたLogEntryオブジェクト</returns>
  /// <exception cref="ArgumentException">無効なEventTypeが指定された場合</exception>
  public LogEntry CreateLogEntry(EventType eventType)
  {
    if (!EventMessages.TryGetValue(eventType, out var messages))
    {
      throw new ArgumentException($"Unsupported EventType: {eventType}", nameof(eventType));
    }

    // ランダムにメッセージを選択
    var selectedMessage = messages[Random.Next(messages.Length)];

    // LogEntryを作成（タイムスタンプは省略でJST現在時刻が自動設定される）
    return new LogEntry(eventType, selectedMessage);
  }

  /// <summary>
  /// ランダムなEventTypeとメッセージでLogEntryオブジェクトを作成します
  /// </summary>
  /// <returns>生成されたLogEntryオブジェクト</returns>
  public LogEntry CreateLogEntry()
  {
    // ランダムにEventTypeを選択
    var eventTypes = Enum.GetValues<EventType>();
    var randomEventType = eventTypes[Random.Next(eventTypes.Length)];

    // 選択されたEventTypeで LogEntry を作成
    return CreateLogEntry(randomEventType);
  }
}
