using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.ValueObjects;

namespace AiDevTest1.Domain.Events
{
    public class LogWrittenToFileEvent : DomainEventBase
    {
        public LogFilePath FilePath { get; }
        public LogEntry LogEntry { get; }

        public LogWrittenToFileEvent(LogFilePath filePath, LogEntry logEntry)
        {
            FilePath = filePath;
            LogEntry = logEntry;
        }
    }
}