using AiDevTest1.Domain.Models;

namespace AiDevTest1.Application.Interfaces
{
  public interface ILogFileHandler
  {
    string GetCurrentLogFilePath();
    Task AppendLogEntryAsync(LogEntry logEntry);
  }
}
