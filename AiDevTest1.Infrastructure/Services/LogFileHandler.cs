using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.ValueObjects;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AiDevTest1.Infrastructure.Services
{
  /// <summary>
  /// <see cref="ILogFileHandler"/>の実装クラス。
  /// 日付ベースのログファイルローテーションを使用してログエントリを管理します。
  /// </summary>
  public class LogFileHandler : ILogFileHandler
  {
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly string _baseDirectory;

    /// <summary>
    /// <see cref="LogFileHandler"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="timeZoneInfo">ログファイル名の日付決定に使用するタイムゾーン。nullの場合は東京標準時を使用します。</param>
    /// <param name="baseDirectory">ログファイルを保存するベースディレクトリ。nullの場合はアプリケーションのベースディレクトリを使用します。</param>
    public LogFileHandler(TimeZoneInfo? timeZoneInfo = null, string? baseDirectory = null)
    {
      _timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
      _baseDirectory = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <inheritdoc/>
    public LogFilePath GetCurrentLogFilePath()
    {
      // JST基準で現在の日付を取得
      var jstNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZoneInfo);

      // LogFilePath Value Objectを使用してパスを生成
      return LogFilePath.CreateForDate(_baseDirectory, jstNow);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// File.AppendAllTextAsyncメソッドを使用してスレッドセーフな書き込みを実現しています。
    /// ファイルが存在しない場合は自動的に作成されます。
    /// </remarks>
    public async Task AppendLogEntryAsync(LogEntry logEntry)
    {
      if (logEntry == null)
      {
        throw new ArgumentNullException(nameof(logEntry));
      }

      var logFilePath = GetCurrentLogFilePath();
      var jsonLine = logEntry.ToJsonLine() + Environment.NewLine;

      // ディレクトリが存在しない場合は作成
      logFilePath.EnsureDirectoryExists();

      // LogFilePathからstringへの暗黙的な変換を利用
      await File.AppendAllTextAsync(logFilePath, jsonLine);
    }
  }
}
