using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.ValueObjects;

namespace AiDevTest1.Application.Interfaces
{
  /// <summary>
  /// ログファイルの操作を抽象化するインターフェース。
  /// ログエントリの永続化とログファイルパスの管理を行います。
  /// </summary>
  public interface ILogFileHandler
  {
    /// <summary>
    /// 現在使用中のログファイルのパスを取得します。
    /// </summary>
    /// <returns>現在のログファイルの完全パス。</returns>
    /// <remarks>
    /// ログファイルは日付やその他の条件に基づいてローテーションされる可能性があります。
    /// このメソッドは常に現在アクティブなログファイルのパスを返します。
    /// </remarks>
    LogFilePath GetCurrentLogFilePath();

    /// <summary>
    /// 指定されたログエントリを現在のログファイルに非同期で追加します。
    /// </summary>
    /// <param name="logEntry">追加するログエントリ。</param>
    /// <returns>非同期操作を表すタスク。</returns>
    /// <exception cref="ArgumentNullException">logEntryがnullの場合。</exception>
    /// <exception cref="IOException">ファイルへの書き込みに失敗した場合。</exception>
    /// <remarks>
    /// このメソッドはスレッドセーフである必要があり、複数の呼び出し元から同時に
    /// 呼び出される可能性があることを考慮して実装する必要があります。
    /// </remarks>
    Task AppendLogEntryAsync(LogEntry logEntry);
  }
}
