using AiDevTest1.Domain.ValueObjects;

namespace AiDevTest1.Domain.Events
{
    public class FileUploadFailedEvent : DomainEventBase
    {
        public LogFilePath FilePath { get; }
        public string ErrorMessage { get; }

        public FileUploadFailedEvent(LogFilePath filePath, string errorMessage)
        {
            FilePath = filePath;
            ErrorMessage = errorMessage;
        }
    }
}