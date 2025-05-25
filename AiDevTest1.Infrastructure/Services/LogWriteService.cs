using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Models;

namespace AiDevTest1.Infrastructure.Services
{
  /// <summary>
  /// ログ書き込みサービスの実装クラス
  /// </summary>
  public class LogWriteService : ILogWriteService
  {
    private readonly ILogEntryFactory _logEntryFactory;
    private readonly ILogFileHandler _logFileHandler;

    /// <summary>
    /// コンストラクタ（依存性注入）
    /// </summary>
    /// <param name="logEntryFactory">ログエントリファクトリ</param>
    /// <param name="logFileHandler">ログファイルハンドラ</param>
    public LogWriteService(ILogEntryFactory logEntryFactory, ILogFileHandler logFileHandler)
    {
      _logEntryFactory = logEntryFactory ?? throw new ArgumentNullException(nameof(logEntryFactory));
      _logFileHandler = logFileHandler ?? throw new ArgumentNullException(nameof(logFileHandler));
    }

    /// <summary>
    /// ランダムなログエントリを生成してファイルに非同期で書き込みます
    /// </summary>
    /// <returns>操作の成功/失敗を表すResult</returns>
    public async Task<Result> WriteLogEntryAsync()
    {
      try
      {
        // ランダムなログエントリを生成
        var logEntry = _logEntryFactory.CreateLogEntry();

        // ファイルに非同期で書き込み
        await _logFileHandler.AppendLogEntryAsync(logEntry);

        return Result.Success();
      }
      catch (Exception ex)
      {
        return Result.Failure($"ログの書き込みに失敗しました: {ex.Message}");
      }
    }
  }
}
