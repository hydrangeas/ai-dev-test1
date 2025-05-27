using System;
using System.Text.RegularExpressions;

namespace AiDevTest1.Domain.ValueObjects
{
  /// <summary>
  /// Azure IoT HubのデバイスIDを表すValue Object
  /// </summary>
  public readonly record struct DeviceId
  {
    private readonly string _value;

    /// <summary>
    /// デバイスIDの最小長
    /// </summary>
    private const int MinLength = 1;

    /// <summary>
    /// デバイスIDの最大長
    /// </summary>
    /// <remarks>
    /// Azure IoT Hubの制限により、デバイスIDは最大128文字
    /// </remarks>
    private const int MaxLength = 128;

    /// <summary>
    /// デバイスIDとして有効な文字のパターン
    /// </summary>
    /// <remarks>
    /// Azure IoT Hubでは以下の文字が使用可能:
    /// - 英数字 (a-z, A-Z, 0-9)
    /// - ハイフン (-)
    /// - アンダースコア (_)
    /// - ピリオド (.)
    /// - コロン (:) ※一部のシナリオで使用
    /// - スラッシュ (/) ※Edgeモジュール形式で使用
    /// - ドルマーク ($) ※予約されたEdgeデバイスIDで使用
    /// </remarks>
    private static readonly Regex ValidDeviceIdPattern = new(@"^[a-zA-Z0-9\-_\.:/$]+$", RegexOptions.Compiled);

    /// <summary>
    /// 予約されたデバイスID（Azureの制約）
    /// </summary>
    private static readonly string[] ReservedIds = { "$edgeAgent", "$edgeHub" };

    /// <summary>
    /// デバイスIDの値を取得します
    /// </summary>
    public string Value => _value ?? throw new InvalidOperationException("DeviceId has not been properly initialized.");

    /// <summary>
    /// DeviceIdのコンストラクタ
    /// </summary>
    /// <param name="value">デバイスID</param>
    /// <exception cref="ArgumentException">デバイスIDが無効な場合</exception>
    public DeviceId(string value)
    {
      ValidateDeviceId(value);
      _value = value;
    }

    /// <summary>
    /// 文字列からDeviceIdへの暗黙的な型変換
    /// </summary>
    /// <param name="value">変換元の文字列</param>
    public static implicit operator DeviceId(string value) => new(value);

    /// <summary>
    /// DeviceIdから文字列への暗黙的な型変換
    /// </summary>
    /// <param name="deviceId">変換元のDeviceId</param>
    public static implicit operator string(DeviceId deviceId) => deviceId.Value;

    /// <summary>
    /// IoT Edgeデバイス用のデバイスIDを作成します
    /// </summary>
    /// <param name="edgeDeviceId">エッジデバイスのID</param>
    /// <param name="moduleId">モジュールID</param>
    /// <returns>エッジモジュール用のDeviceId</returns>
    public static DeviceId CreateForEdgeModule(string edgeDeviceId, string moduleId)
    {
      if (string.IsNullOrWhiteSpace(edgeDeviceId))
      {
        throw new ArgumentException("Edge device ID cannot be null or empty.", nameof(edgeDeviceId));
      }

      if (string.IsNullOrWhiteSpace(moduleId))
      {
        throw new ArgumentException("Module ID cannot be null or empty.", nameof(moduleId));
      }

      // IoT Edgeのモジュール形式: {device_id}/{module_id}
      return new DeviceId($"{edgeDeviceId}/{moduleId}");
    }

    /// <summary>
    /// デバイスIDが予約されたIDかどうかを確認します
    /// </summary>
    /// <returns>予約されたIDの場合はtrue</returns>
    public bool IsReservedId()
    {
      var currentValue = Value;
      return Array.Exists(ReservedIds, reserved =>
          string.Equals(currentValue, reserved, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// デバイスIDがEdgeモジュール形式かどうかを確認します
    /// </summary>
    /// <returns>Edgeモジュール形式の場合はtrue</returns>
    public bool IsEdgeModule()
    {
      return Value.Contains('/');
    }

    /// <summary>
    /// デバイスIDの妥当性を検証します
    /// </summary>
    /// <param name="value">検証するデバイスID</param>
    /// <exception cref="ArgumentException">デバイスIDが無効な場合</exception>
    private static void ValidateDeviceId(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        throw new ArgumentException("Device ID cannot be null, empty, or whitespace.", nameof(value));
      }

      if (value.Length < MinLength || value.Length > MaxLength)
      {
        throw new ArgumentException($"Device ID length must be between {MinLength} and {MaxLength} characters. Actual length: {value.Length}.", nameof(value));
      }

      if (!ValidDeviceIdPattern.IsMatch(value))
      {
        throw new ArgumentException($"Device ID contains invalid characters. Only alphanumeric characters, hyphens, underscores, periods, colons, forward slashes, and dollar signs are allowed. Value: '{value}'", nameof(value));
      }

      // 先頭・末尾の特殊文字のチェック
      if (value.StartsWith('.') || value.EndsWith('.'))
      {
        throw new ArgumentException("Device ID cannot start or end with a period.", nameof(value));
      }

      if (value.StartsWith('-') || value.EndsWith('-'))
      {
        throw new ArgumentException("Device ID cannot start or end with a hyphen.", nameof(value));
      }

      if (value.StartsWith('_') || value.EndsWith('_'))
      {
        throw new ArgumentException("Device ID cannot start or end with an underscore.", nameof(value));
      }

      // 連続する特殊文字のチェック
      if (value.Contains("..") || value.Contains("--") || value.Contains("__"))
      {
        throw new ArgumentException("Device ID cannot contain consecutive special characters.", nameof(value));
      }

      // Edgeモジュール形式の検証
      if (value.Contains('/'))
      {
        var parts = value.Split('/');
        if (parts.Length != 2)
        {
          throw new ArgumentException("Edge module device ID must be in the format 'deviceId/moduleId'.", nameof(value));
        }

        if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
          throw new ArgumentException("Both device ID and module ID must be non-empty in Edge module format.", nameof(value));
        }
      }
    }

    /// <summary>
    /// 文字列表現を返します
    /// </summary>
    /// <returns>デバイスIDの文字列表現</returns>
    public override string ToString() => Value;
  }
}
