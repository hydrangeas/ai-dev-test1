using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiDevTest1.Domain.Models;

/// <summary>
/// ログの単一レコードを表すクラス
/// </summary>
public class LogEntry
{
  /// <summary>
  /// JSON シリアライズ用の共有設定（パフォーマンス最適化）
  /// </summary>
  private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() },
    WriteIndented = false
  };

  /// <summary>
  /// JST タイムゾーン情報（パフォーマンス最適化）
  /// </summary>
  private static readonly TimeZoneInfo JstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

  /// <summary>
  /// ログエントリのタイムスタンプ
  /// </summary>
  public DateTimeOffset Timestamp { get; init; }

  /// <summary>
  /// イベントタイプ
  /// </summary>
  public EventType EventType { get; init; }

  /// <summary>
  /// コメント
  /// </summary>
  public string Comment { get; init; } = string.Empty;

  /// <summary>
  /// デフォルトコンストラクタ（ORM用）
  /// </summary>
  private LogEntry()
  {
  }

  /// <summary>
  /// パラメータ付きコンストラクタ
  /// </summary>
  /// <param name="eventType">イベントタイプ</param>
  /// <param name="comment">コメント</param>
  /// <param name="timestamp">タイムスタンプ（省略時は現在時刻のJST）</param>
  public LogEntry(EventType eventType, string comment, DateTimeOffset? timestamp = null)
  {
    if (!Enum.IsDefined(typeof(EventType), eventType))
    {
      throw new ArgumentException($"Invalid EventType value: {eventType}", nameof(eventType));
    }

    EventType = eventType;
    Comment = comment ?? string.Empty;
    Timestamp = timestamp ?? GetJstNow();
  }

  /// <summary>
  /// JSON Lines形式の文字列に変換します
  /// </summary>
  /// <returns>JSON Lines形式の文字列</returns>
  public string ToJsonLine()
  {
    // JSTタイムゾーンに変換してからシリアライズ
    var jstTimestamp = ConvertToJst(Timestamp);
    var jsonObject = new
    {
      timestamp = jstTimestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
      eventType = EventType.ToString(),
      comment = Comment
    };

    return JsonSerializer.Serialize(jsonObject, CachedJsonSerializerOptions);
  }

  /// <summary>
  /// 現在のJST時刻を取得します
  /// </summary>
  /// <returns>JST時刻のDateTimeOffset</returns>
  private static DateTimeOffset GetJstNow()
  {
    return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, JstTimeZone);
  }

  /// <summary>
  /// DateTimeOffsetをJSTに変換します
  /// </summary>
  /// <param name="dateTime">変換対象の日時</param>
  /// <returns>JSTに変換された日時</returns>
  private static DateTimeOffset ConvertToJst(DateTimeOffset dateTime)
  {
    return TimeZoneInfo.ConvertTime(dateTime, JstTimeZone);
  }
}
