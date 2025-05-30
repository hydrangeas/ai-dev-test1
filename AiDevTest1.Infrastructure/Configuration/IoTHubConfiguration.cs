using AiDevTest1.Domain.ValueObjects;
using System;

namespace AiDevTest1.Infrastructure.Configuration
{
  /// <summary>
  /// Azure IoT Hub接続設定
  /// </summary>
  public class IoTHubConfiguration
  {
    /// <summary>
    /// IoT Hub接続文字列
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// デバイスID
    /// </summary>
    public DeviceId DeviceId { get; }

    /// <summary>
    /// IoT Hub設定を初期化します
    /// </summary>
    /// <param name="connectionString">IoT Hub接続文字列</param>
    /// <param name="deviceId">デバイスID</param>
    /// <exception cref="ArgumentException">接続文字列が空または無効な場合</exception>
    public IoTHubConfiguration(string connectionString, DeviceId deviceId)
    {
      if (string.IsNullOrWhiteSpace(connectionString))
        throw new ArgumentException("接続文字列が空です。", nameof(connectionString));

      ConnectionString = connectionString;
      DeviceId = deviceId;
    }

    /// <summary>
    /// 設定用のパラメーターレスコンストラクタ（プロパティセッター用）
    /// </summary>
    public IoTHubConfiguration()
    {
      ConnectionString = string.Empty;
      DeviceId = new DeviceId("default-device");
    }

    /// <summary>
    /// 設定値の妥当性を検証します
    /// </summary>
    /// <returns>設定が有効な場合はtrue、無効な場合はfalse</returns>
    public bool IsValid()
    {
      return !string.IsNullOrWhiteSpace(ConnectionString) &&
             DeviceId != null &&
             !string.IsNullOrWhiteSpace(DeviceId.Value);
    }
  }
}
