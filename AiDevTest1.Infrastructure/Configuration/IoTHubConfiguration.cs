using AiDevTest1.Domain.ValueObjects;
using System;

namespace AiDevTest1.Infrastructure.Configuration
{
  /// <summary>
  /// Azure IoT Hub接続設定
  /// </summary>
  public record IoTHubConfiguration
  {
    /// <summary>
    /// IoT Hub接続文字列
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// デバイスID
    /// </summary>
    public DeviceId DeviceId { get; init; } = new DeviceId("default-device");

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
    /// 設定用のパラメーターレスコンストラクタ
    /// </summary>
    public IoTHubConfiguration()
    {
    }
  }
}
