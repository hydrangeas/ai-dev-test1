using System.Diagnostics.CodeAnalysis;
using AiDevTest1.Domain.Exceptions;

namespace AiDevTest1.Domain.ValueObjects
{
  /// <summary>
  /// ログファイルのパスを表すValue Object。
  /// </summary>
  /// <remarks>
  /// このValue Objectは、ログファイルパスの妥当性を保証し、
  /// ドメインロジックにおける型安全性を提供します。
  /// </remarks>
  public readonly record struct LogFilePath
  {
    private readonly string _value;

    /// <summary>
    /// パスの文字列値を取得します。
    /// </summary>
    public string Value => _value ?? throw new InvalidOperationException("LogFilePath has not been properly initialized.");

    /// <summary>
    /// ファイル名（拡張子を含む）を取得します。
    /// </summary>
    public string FileName => Path.GetFileName(Value);

    /// <summary>
    /// ディレクトリパスを取得します。
    /// </summary>
    public string DirectoryPath => Path.GetDirectoryName(Value) ?? string.Empty;

    /// <summary>
    /// LogFilePathの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="value">ログファイルのパス。</param>
    /// <exception cref="DomainException">パスが無効な場合。</exception>
    public LogFilePath(string value)
    {
      ValidatePath(value);
      _value = Path.GetFullPath(value); // 絶対パスに正規化
    }

    /// <summary>
    /// パスの妥当性を検証します。
    /// </summary>
    /// <param name="path">検証するパス。</param>
    /// <exception cref="DomainException">パスが無効な場合。</exception>
    private static void ValidatePath([NotNull] string? path)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        throw new DomainException(
            "ログファイルパスが空です。",
            "LogFilePath.Empty");
      }

      try
      {
        // パスの形式が有効かチェック
        _ = Path.GetFullPath(path);
      }
      catch (Exception ex) when (ex is ArgumentException or PathTooLongException or NotSupportedException)
      {
        throw new DomainException(
            $"無効なログファイルパス形式です: {path}",
            "LogFilePath.InvalidFormat",
            innerException: ex);
      }

      // ログファイルの拡張子チェック
      var extension = Path.GetExtension(path);
      if (!string.Equals(extension, ".log", StringComparison.OrdinalIgnoreCase))
      {
        throw new DomainException(
            $"ログファイルは.log拡張子である必要があります: {path}",
            "LogFilePath.InvalidExtension");
      }
    }

    /// <summary>
    /// ファイルが存在するかどうかを確認します。
    /// </summary>
    /// <returns>ファイルが存在する場合はtrue。</returns>
    public bool Exists() => File.Exists(Value);

    /// <summary>
    /// ディレクトリが存在しない場合は作成します。
    /// </summary>
    public void EnsureDirectoryExists()
    {
      var directory = DirectoryPath;
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }
    }

    /// <summary>
    /// 文字列をLogFilePathに暗黙的に変換します。
    /// </summary>
    /// <param name="value">変換する文字列。</param>
    public static implicit operator LogFilePath(string value) => new(value);

    /// <summary>
    /// LogFilePathを文字列に暗黙的に変換します。
    /// </summary>
    /// <param name="logFilePath">変換するLogFilePath。</param>
    public static implicit operator string(LogFilePath logFilePath) => logFilePath.Value;

    /// <summary>
    /// パスの文字列表現を返します。
    /// </summary>
    /// <returns>ログファイルのパス。</returns>
    public override string ToString() => Value;

    /// <summary>
    /// 指定されたベースディレクトリと日付から標準的なログファイルパスを作成します。
    /// </summary>
    /// <param name="baseDirectory">ベースディレクトリ。</param>
    /// <param name="date">ログファイルの日付。</param>
    /// <returns>作成されたLogFilePath。</returns>
    public static LogFilePath CreateForDate(string baseDirectory, DateTime date)
    {
      if (string.IsNullOrWhiteSpace(baseDirectory))
      {
        throw new ArgumentException("ベースディレクトリが指定されていません。", nameof(baseDirectory));
      }

      var fileName = $"{date:yyyy-MM-dd}.log";
      var path = Path.Combine(baseDirectory, fileName);
      return new LogFilePath(path);
    }

    /// <summary>
    /// 現在の日付で標準的なログファイルパスを作成します。
    /// </summary>
    /// <param name="baseDirectory">ベースディレクトリ。</param>
    /// <returns>作成されたLogFilePath。</returns>
    public static LogFilePath CreateForToday(string baseDirectory)
        => CreateForDate(baseDirectory, DateTime.Now);
  }
}
