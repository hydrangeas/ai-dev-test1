using AiDevTest1.Application.Interfaces;
using System;
using System.IO;

namespace AiDevTest1.Infrastructure.Services
{
  public class LogFileHandler : ILogFileHandler
  {
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly string _baseDirectory;

    public LogFileHandler(TimeZoneInfo? timeZoneInfo = null, string? baseDirectory = null)
    {
      _timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
      _baseDirectory = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
    }

    public string GetCurrentLogFilePath()
    {
      // JST基準で現在の日付を取得
      var jstNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZoneInfo);

      // yyyy-MM-dd.log形式でファイル名を生成
      var fileName = $"{jstNow:yyyy-MM-dd}.log";

      // 指定されたベースディレクトリ直下のパスを生成
      var filePath = Path.Combine(_baseDirectory, fileName);

      return filePath;
    }
  }
}
