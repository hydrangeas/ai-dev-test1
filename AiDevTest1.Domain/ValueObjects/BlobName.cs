using System;
using System.Text.RegularExpressions;

namespace AiDevTest1.Domain.ValueObjects
{
  /// <summary>
  /// Azure Blob Storage用のBlob名を表すValue Object
  /// </summary>
  public readonly record struct BlobName
  {
    private readonly string _value;

    /// <summary>
    /// Blob名の最小長
    /// </summary>
    private const int MinLength = 1;

    /// <summary>
    /// Blob名の最大長
    /// </summary>
    private const int MaxLength = 1024;

    /// <summary>
    /// Azure Blob名として有効な文字のパターン
    /// </summary>
    /// <remarks>
    /// Azure Blob Storageでは以下の文字が使用可能:
    /// - 英数字 (a-z, A-Z, 0-9)
    /// - ハイフン (-)
    /// - アンダースコア (_)
    /// - ピリオド (.)
    /// - スラッシュ (/)
    /// </remarks>
    private static readonly Regex ValidBlobNamePattern = new(@"^[a-zA-Z0-9\-_\./]+$", RegexOptions.Compiled);

    /// <summary>
    /// 予約されたBlob名（Azureの制約）
    /// </summary>
    private static readonly string[] ReservedNames = { ".", ".." };

    /// <summary>
    /// Blob名の値を取得します
    /// </summary>
    public string Value => _value ?? throw new InvalidOperationException("BlobName has not been properly initialized.");

    /// <summary>
    /// デバイスIDを含む完全なBlob名を取得します
    /// </summary>
    /// <param name="deviceId">デバイスID</param>
    /// <returns>デバイスID/Blob名の形式の文字列</returns>
    public string GetFullBlobName(DeviceId deviceId)
    {
      return $"{deviceId.Value}/{Value}";
    }

    /// <summary>
    /// BlobNameのコンストラクタ
    /// </summary>
    /// <param name="value">Blob名</param>
    /// <exception cref="ArgumentException">Blob名が無効な場合</exception>
    public BlobName(string value)
    {
      ValidateBlobName(value);
      _value = value;
    }

    /// <summary>
    /// 文字列からBlobNameへの暗黙的な型変換
    /// </summary>
    /// <param name="value">変換元の文字列</param>
    public static implicit operator BlobName(string value) => new(value);

    /// <summary>
    /// BlobNameから文字列への暗黙的な型変換
    /// </summary>
    /// <param name="blobName">変換元のBlobName</param>
    public static implicit operator string(BlobName blobName) => blobName.Value;

    /// <summary>
    /// ログファイル用のBlob名を作成します
    /// </summary>
    /// <param name="date">日付</param>
    /// <returns>yyyy-MM-dd.log形式のBlobName</returns>
    public static BlobName CreateForLogFile(DateTime date)
    {
      var fileName = $"{date:yyyy-MM-dd}.log";
      return new BlobName(fileName);
    }

    /// <summary>
    /// 今日の日付でログファイル用のBlob名を作成します
    /// </summary>
    /// <returns>今日の日付のログファイル用BlobName</returns>
    public static BlobName CreateForTodayLogFile()
    {
      return CreateForLogFile(DateTime.Today);
    }

    /// <summary>
    /// Blob名の妥当性を検証します
    /// </summary>
    /// <param name="value">検証するBlob名</param>
    /// <exception cref="ArgumentException">Blob名が無効な場合</exception>
    private static void ValidateBlobName(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        throw new ArgumentException("Blob name cannot be null, empty, or whitespace.", nameof(value));
      }

      if (value.Length < MinLength || value.Length > MaxLength)
      {
        throw new ArgumentException($"Blob name length must be between {MinLength} and {MaxLength} characters. Actual length: {value.Length}.", nameof(value));
      }

      if (!ValidBlobNamePattern.IsMatch(value))
      {
        throw new ArgumentException($"Blob name contains invalid characters. Only alphanumeric characters, hyphens, underscores, periods, and forward slashes are allowed. Value: '{value}'", nameof(value));
      }

      // 予約名のチェック
      if (Array.Exists(ReservedNames, reserved => string.Equals(value, reserved, StringComparison.OrdinalIgnoreCase)))
      {
        throw new ArgumentException($"Blob name '{value}' is reserved and cannot be used.", nameof(value));
      }

      // 連続するスラッシュのチェック
      if (value.Contains("//"))
      {
        throw new ArgumentException("Blob name cannot contain consecutive forward slashes.", nameof(value));
      }

      // 先頭・末尾のスラッシュのチェック
      if (value.StartsWith('/') || value.EndsWith('/'))
      {
        throw new ArgumentException("Blob name cannot start or end with a forward slash.", nameof(value));
      }

      // 先頭・末尾のピリオドのチェック（Azureの制約）
      var segments = value.Split('/');
      foreach (var segment in segments)
      {
        if (segment.StartsWith('.') || segment.EndsWith('.'))
        {
          throw new ArgumentException("Blob name segments cannot start or end with a period.", nameof(value));
        }
      }
    }

    /// <summary>
    /// 文字列表現を返します
    /// </summary>
    /// <returns>Blob名の文字列表現</returns>
    public override string ToString() => Value;
  }
}
