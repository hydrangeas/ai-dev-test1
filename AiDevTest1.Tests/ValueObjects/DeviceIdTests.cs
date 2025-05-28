using System;
using AiDevTest1.Domain.ValueObjects;
using Xunit;

namespace AiDevTest1.Tests.ValueObjects
{
  /// <summary>
  /// DeviceId Value Objectのユニットテスト
  /// </summary>
  public class DeviceIdTests
  {
    #region コンストラクタのテスト

    [Theory]
    [InlineData("device1")]
    [InlineData("IoT-Device-001")]
    [InlineData("sensor_01")]
    [InlineData("device.test")]
    [InlineData("device:test")]
    [InlineData("123456789")]
    [InlineData("a")] // 最小長
    [InlineData("UPPERCASE-DEVICE")]
    [InlineData("MixedCase.Device")]
    [InlineData("device-with-multiple.special_characters:test")]
    public void Constructor_WithValidDeviceId_ShouldCreateInstance(string validId)
    {
      // Act
      var deviceId = new DeviceId(validId);

      // Assert
      Assert.Equal(validId, deviceId.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithNullOrWhiteSpace_ShouldThrowArgumentException(string? invalidId)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new DeviceId(invalidId!));
      Assert.Contains("Device ID cannot be null, empty, or whitespace", ex.Message);
      Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithTooLongId_ShouldThrowArgumentException()
    {
      // Arrange
      var tooLongId = new string('a', 129); // 最大長128を超える

      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new DeviceId(tooLongId));
      Assert.Contains("Device ID length must be between 1 and 128 characters", ex.Message);
      Assert.Contains("Actual length: 129", ex.Message);
    }

    [Theory]
    [InlineData("device with spaces")]
    [InlineData("device@name")]
    [InlineData("device#name")]
    [InlineData("device%name")]
    [InlineData("device&name")]
    [InlineData("device*name")]
    [InlineData("device(name)")]
    [InlineData("device[name]")]
    [InlineData("device{name}")]
    [InlineData("device<name>")]
    [InlineData("device>name")]
    [InlineData("device|name")]
    [InlineData("device\\name")]
    [InlineData("device\"name")]
    [InlineData("device'name")]
    [InlineData("device;name")]
    [InlineData("device,name")]
    [InlineData("device?name")]
    [InlineData("device=name")]
    [InlineData("device+name")]
    public void Constructor_WithInvalidCharacters_ShouldThrowArgumentException(string invalidId)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new DeviceId(invalidId));
      Assert.Contains("Device ID contains invalid characters", ex.Message);
      Assert.Contains($"Value: '{invalidId}'", ex.Message);
    }

    [Theory]
    [InlineData(".device")]
    [InlineData("device.")]
    [InlineData("-device")]
    [InlineData("device-")]
    [InlineData("_device")]
    [InlineData("device_")]
    public void Constructor_WithInvalidStartOrEnd_ShouldThrowArgumentException(string invalidId)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new DeviceId(invalidId));
      Assert.Contains("cannot start or end with", ex.Message);
    }

    [Theory]
    [InlineData("device..test")]
    [InlineData("device--test")]
    [InlineData("device__test")]
    [InlineData("test...device")]
    [InlineData("test---device")]
    [InlineData("test___device")]
    public void Constructor_WithConsecutiveSpecialCharacters_ShouldThrowArgumentException(string invalidId)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new DeviceId(invalidId));
      Assert.Contains("Device ID cannot contain consecutive special characters", ex.Message);
    }

    #endregion

    #region Edge Module形式のテスト

    [Theory]
    [InlineData("edgeDevice/module1")]
    [InlineData("edge-device-001/temperature-module")]
    [InlineData("iot.edge.device/sensor_module")]
    public void Constructor_WithValidEdgeModuleFormat_ShouldCreateInstance(string validEdgeId)
    {
      // Act
      var deviceId = new DeviceId(validEdgeId);

      // Assert
      Assert.Equal(validEdgeId, deviceId.Value);
      Assert.True(deviceId.IsEdgeModule());
    }

    [Theory]
    [InlineData("device/module/extra")]
    [InlineData("device//module")]
    [InlineData("/module")]
    [InlineData("device/")]
    [InlineData("device/module/")]
    public void Constructor_WithInvalidEdgeModuleFormat_ShouldThrowArgumentException(string invalidEdgeId)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => new DeviceId(invalidEdgeId));
    }

    [Fact]
    public void CreateForEdgeModule_WithValidParameters_ShouldCreateEdgeModuleId()
    {
      // Arrange
      var edgeDeviceId = "edge-device-001";
      var moduleId = "temperature-module";

      // Act
      var deviceId = DeviceId.CreateForEdgeModule(edgeDeviceId, moduleId);

      // Assert
      Assert.Equal($"{edgeDeviceId}/{moduleId}", deviceId.Value);
      Assert.True(deviceId.IsEdgeModule());
    }

    [Theory]
    [InlineData(null, "module")]
    [InlineData("", "module")]
    [InlineData(" ", "module")]
    [InlineData("device", null)]
    [InlineData("device", "")]
    [InlineData("device", " ")]
    public void CreateForEdgeModule_WithInvalidParameters_ShouldThrowArgumentException(string? edgeDeviceId, string? moduleId)
    {
      // Act & Assert
      var ex = Assert.Throws<ArgumentException>(() => DeviceId.CreateForEdgeModule(edgeDeviceId!, moduleId!));
      Assert.Contains("cannot be null or empty", ex.Message);
    }

    #endregion

    #region 暗黙的型変換のテスト

    [Fact]
    public void ImplicitConversion_FromString_ShouldCreateDeviceId()
    {
      // Arrange
      string deviceName = "test-device";

      // Act
      DeviceId deviceId = deviceName;

      // Assert
      Assert.Equal(deviceName, deviceId.Value);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
      // Arrange
      var deviceId = new DeviceId("test-device");

      // Act
      string value = deviceId;

      // Assert
      Assert.Equal("test-device", value);
    }

    #endregion

    #region IsReservedIdメソッドのテスト

    [Theory]
    [InlineData("$edgeAgent")]
    [InlineData("$edgeHub")]
    [InlineData("$EDGEAGENT")] // 大文字小文字を区別しない
    [InlineData("$EDGEHUB")]
    [InlineData("$EdgeAgent")]
    [InlineData("$EdgeHub")]
    public void IsReservedId_WithReservedIds_ShouldReturnTrue(string reservedId)
    {
      // Arrange
      var deviceId = new DeviceId(reservedId);

      // Act
      var isReserved = deviceId.IsReservedId();

      // Assert
      Assert.True(isReserved);
    }

    [Theory]
    [InlineData("edgeAgent")]
    [InlineData("edgeHub")]
    [InlineData("$edge")]
    [InlineData("$otherDevice")]
    [InlineData("normal-device")]
    public void IsReservedId_WithNonReservedIds_ShouldReturnFalse(string nonReservedId)
    {
      // Arrange
      var deviceId = new DeviceId(nonReservedId);

      // Act
      var isReserved = deviceId.IsReservedId();

      // Assert
      Assert.False(isReserved);
    }

    #endregion

    #region IsEdgeModuleメソッドのテスト

    [Theory]
    [InlineData("device/module", true)]
    [InlineData("edge-device/temp-sensor", true)]
    [InlineData("device.test/module.test", true)]
    [InlineData("regular-device", false)]
    [InlineData("device-no-module", false)]
    [InlineData("device.test", false)]
    public void IsEdgeModule_ShouldReturnCorrectValue(string deviceIdValue, bool expected)
    {
      // Arrange
      var deviceId = new DeviceId(deviceIdValue);

      // Act
      var isEdgeModule = deviceId.IsEdgeModule();

      // Assert
      Assert.Equal(expected, isEdgeModule);
    }

    #endregion

    #region ToStringメソッドのテスト

    [Theory]
    [InlineData("test-device")]
    [InlineData("device/module")]
    [InlineData("IoT-Device-001")]
    public void ToString_ShouldReturnValue(string deviceIdValue)
    {
      // Arrange
      var deviceId = new DeviceId(deviceIdValue);

      // Act
      var result = deviceId.ToString();

      // Assert
      Assert.Equal(deviceIdValue, result);
    }

    #endregion

    #region Equalityのテスト

    [Fact]
    public void Equality_SameDeviceIds_ShouldBeEqual()
    {
      // Arrange
      var deviceId1 = new DeviceId("test-device");
      var deviceId2 = new DeviceId("test-device");

      // Act & Assert
      Assert.Equal(deviceId1, deviceId2);
      Assert.True(deviceId1 == deviceId2);
      Assert.False(deviceId1 != deviceId2);
      Assert.Equal(deviceId1.GetHashCode(), deviceId2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentDeviceIds_ShouldNotBeEqual()
    {
      // Arrange
      var deviceId1 = new DeviceId("device1");
      var deviceId2 = new DeviceId("device2");

      // Act & Assert
      Assert.NotEqual(deviceId1, deviceId2);
      Assert.False(deviceId1 == deviceId2);
      Assert.True(deviceId1 != deviceId2);
    }

    #endregion

    #region 初期化されていないインスタンスのテスト

    [Fact]
    public void Value_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
      // Arrange
      var deviceId = default(DeviceId);

      // Act & Assert
      var ex = Assert.Throws<InvalidOperationException>(() => deviceId.Value);
      Assert.Equal("DeviceId has not been properly initialized.", ex.Message);
    }

    #endregion

    #region 実際のAzure IoT Hubシナリオのテスト

    [Theory]
    [InlineData("iot-device-001")]
    [InlineData("temperature-sensor-floor-2")]
    [InlineData("gateway.building.a")]
    [InlineData("dev:test:001")]
    [InlineData("prod-device-001")]
    public void Constructor_WithRealWorldDeviceIds_ShouldCreateInstance(string realWorldId)
    {
      // Act
      var deviceId = new DeviceId(realWorldId);

      // Assert
      Assert.Equal(realWorldId, deviceId.Value);
    }

    [Fact]
    public void Constructor_WithMaxLengthDeviceId_ShouldCreateInstance()
    {
      // Arrange
      var maxLengthId = new string('a', 128);

      // Act
      var deviceId = new DeviceId(maxLengthId);

      // Assert
      Assert.Equal(maxLengthId, deviceId.Value);
      Assert.Equal(128, deviceId.Value.Length);
    }

    #endregion
  }
}
