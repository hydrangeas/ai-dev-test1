using AiDevTest1.Application.Interfaces;
using System;
using System.IO;

namespace AiDevTest1.Infrastructure.Services
{
  public class LogFileHandler : ILogFileHandler
  {
    private static readonly TimeZoneInfo JstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    public string GetCurrentLogFilePath()
    {
      // JST基準で現在の日付を取得
      var jstNow = TimeZoneInfo.ConvertTime(DateTime.Now, JstTimeZone);

      // yyyy-MM-dd.log形式でファイル名を生成
      var fileName = $"{jstNow:yyyy-MM-dd}.log";

      // アプリケーション実行ディレクトリ直下のパスを生成
      var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
      var filePath = Path.Combine(baseDirectory, fileName);

      return filePath;
    }
  }
}
