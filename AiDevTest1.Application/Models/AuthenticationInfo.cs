using AiDevTest1.Domain.ValueObjects;

namespace AiDevTest1.Application.Models
{
  public class AuthenticationInfo
  {
    public string ConnectionString { get; set; } = string.Empty;
    public DeviceId DeviceId { get; set; } = new DeviceId("default-device");
  }
}
