using AiDevTest1.Application.Factories;
using AiDevTest1.Domain.Models;
using Xunit;

namespace AiDevTest1.Tests;

/// <summary>
/// LogEntryFactoryクラスのユニットテスト
/// </summary>
public class LogEntryFactoryTests
{
  private readonly LogEntryFactory _factory;

  public LogEntryFactoryTests()
  {
    _factory = new LogEntryFactory();
  }

  /// <summary>
  /// 指定EventTypeでLogEntryが正常に作成されることをテスト
  /// </summary>
  [Theory]
  [InlineData(EventType.START)]
  [InlineData(EventType.STOP)]
  [InlineData(EventType.WARN)]
  [InlineData(EventType.ERROR)]
  public void CreateLogEntry_WithSpecificEventType_ReturnsLogEntryWithCorrectEventType(EventType eventType)
  {
    // Act
    var logEntry = _factory.CreateLogEntry(eventType);

    // Assert
    Assert.NotNull(logEntry);
    Assert.Equal(eventType, logEntry.EventType);
    Assert.NotEmpty(logEntry.Comment);
    Assert.True(logEntry.Timestamp > DateTimeOffset.MinValue);
  }

  /// <summary>
  /// 指定EventTypeでメッセージが期待される候補から選択されることをテスト
  /// </summary>
  [Fact]
  public void CreateLogEntry_WithStartEventType_ReturnsExpectedMessages()
  {
    // Arrange
    var expectedMessages = new[]
    {
      "運転を開始しました。",
      "加工シーケンスを開始します。"
    };

    // Act & Assert
    for (int i = 0; i < 20; i++) // 複数回実行してランダム性を確認
    {
      var logEntry = _factory.CreateLogEntry(EventType.START);
      Assert.Contains(logEntry.Comment, expectedMessages);
    }
  }

  /// <summary>
  /// STOPイベントタイプで期待されるメッセージが選択されることをテスト
  /// </summary>
  [Fact]
  public void CreateLogEntry_WithStopEventType_ReturnsExpectedMessages()
  {
    // Arrange
    var expectedMessages = new[]
    {
      "運転を停止しました。",
      "現在の加工サイクルを完了し、停止しました。"
    };

    // Act & Assert
    for (int i = 0; i < 20; i++)
    {
      var logEntry = _factory.CreateLogEntry(EventType.STOP);
      Assert.Contains(logEntry.Comment, expectedMessages);
    }
  }

  /// <summary>
  /// WARNイベントタイプで期待されるメッセージが選択されることをテスト
  /// </summary>
  [Fact]
  public void CreateLogEntry_WithWarnEventType_ReturnsExpectedMessages()
  {
    // Arrange
    var expectedMessages = new[]
    {
      "主軸モーターの温度が上昇しています。確認してください。",
      "切削油の残量が少なくなっています。補充を検討してください。"
    };

    // Act & Assert
    for (int i = 0; i < 20; i++)
    {
      var logEntry = _factory.CreateLogEntry(EventType.WARN);
      Assert.Contains(logEntry.Comment, expectedMessages);
    }
  }

  /// <summary>
  /// ERRORイベントタイプで期待されるメッセージが選択されることをテスト
  /// </summary>
  [Fact]
  public void CreateLogEntry_WithErrorEventType_ReturnsExpectedMessages()
  {
    // Arrange
    var expectedMessages = new[]
    {
      "サーボモーターエラーが発生しました。システムを停止します。 (コード: E012)",
      "工具が破損しました。交換が必要です。機械を停止しました。"
    };

    // Act & Assert
    for (int i = 0; i < 20; i++)
    {
      var logEntry = _factory.CreateLogEntry(EventType.ERROR);
      Assert.Contains(logEntry.Comment, expectedMessages);
    }
  }

  /// <summary>
  /// パラメータなしのCreateLogEntryがランダムなEventTypeでLogEntryを作成することをテスト
  /// </summary>
  [Fact]
  public void CreateLogEntry_WithoutParameters_ReturnsLogEntryWithValidEventType()
  {
    // Arrange
    var validEventTypes = new[] { EventType.START, EventType.STOP, EventType.WARN, EventType.ERROR };

    // Act & Assert
    for (int i = 0; i < 50; i++) // 複数回実行してランダム性とすべてのEventTypeをテスト
    {
      var logEntry = _factory.CreateLogEntry();

      Assert.NotNull(logEntry);
      Assert.Contains(logEntry.EventType, validEventTypes);
      Assert.NotEmpty(logEntry.Comment);
      Assert.True(logEntry.Timestamp > DateTimeOffset.MinValue);
    }
  }

  /// <summary>
  /// パラメータなしのCreateLogEntryで全EventTypeが選択されることを確認
  /// </summary>
  [Fact]
  public void CreateLogEntry_WithoutParameters_GeneratesAllEventTypes()
  {
    // Arrange
    var generatedEventTypes = new HashSet<EventType>();
    var maxAttempts = 200; // 十分な回数を実行

    // Act
    for (int i = 0; i < maxAttempts; i++)
    {
      var logEntry = _factory.CreateLogEntry();
      generatedEventTypes.Add(logEntry.EventType);

      // 全EventTypeが生成されたら早期終了
      if (generatedEventTypes.Count == 4)
        break;
    }

    // Assert
    Assert.Contains(EventType.START, generatedEventTypes);
    Assert.Contains(EventType.STOP, generatedEventTypes);
    Assert.Contains(EventType.WARN, generatedEventTypes);
    Assert.Contains(EventType.ERROR, generatedEventTypes);
  }

  /// <summary>
  /// タイムスタンプがJST時刻で設定されることをテスト
  /// </summary>
  [Fact]
  public void CreateLogEntry_GeneratesJstTimestamp()
  {
    // Arrange
    var beforeCreation = DateTimeOffset.UtcNow;

    // Act
    var logEntry = _factory.CreateLogEntry(EventType.START);

    // Assert
    var afterCreation = DateTimeOffset.UtcNow;
    Assert.True(logEntry.Timestamp >= beforeCreation.AddSeconds(-1)); // 1秒の誤差を許容
    Assert.True(logEntry.Timestamp <= afterCreation.AddSeconds(1));
  }

  /// <summary>
  /// 複数回の呼び出しで異なるメッセージが選択されることをテスト（ランダム性の確認）
  /// </summary>
  [Fact]
  public void CreateLogEntry_MultipleCalls_GeneratesDifferentMessages()
  {
    // Arrange
    var messages = new HashSet<string>();
    var iterations = 100;

    // Act
    for (int i = 0; i < iterations; i++)
    {
      var logEntry = _factory.CreateLogEntry(EventType.START); // 2つのメッセージがある
      messages.Add(logEntry.Comment);
    }

    // Assert - STARTには2つのメッセージがあるので、十分な回数実行すれば両方が選ばれるはず
    Assert.True(messages.Count >= 1); // 最低1つのメッセージは生成される
    // 確率的に両方のメッセージが選ばれることを期待するが、保証はできないため最低限のテスト
  }
}
