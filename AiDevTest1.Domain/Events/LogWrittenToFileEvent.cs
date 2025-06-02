using AiDevTest1.Domain.Models;

namespace AiDevTest1.Domain.Events
{
    public class LogWrittenToFileEvent : DomainEventBase
    {
        public string FilePath { get; }
        public LogEntry LogEntry { get; }

        public LogWrittenToFileEvent(string filePath, LogEntry logEntry)
        {
            FilePath = filePath;
            LogEntry = logEntry;
        }
    }
}