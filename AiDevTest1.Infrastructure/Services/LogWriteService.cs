using AiDevTest1.Application.Interfaces;
using System;

namespace AiDevTest1.Infrastructure.Services
{
  public class LogWriteService : ILogWriteService
  {
    public void Log(string message)
    {
      // ダミー実装: コンソールに出力するだけ
      Console.WriteLine($"[Log]: {message}");
    }
  }
}
