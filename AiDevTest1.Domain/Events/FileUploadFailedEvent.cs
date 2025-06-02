using System;
using AiDevTest1.Domain.ValueObjects;

namespace AiDevTest1.Domain.Events
{
    public class FileUploadFailedEvent : DomainEventBase
    {
        public LogFilePath FilePath { get; }
        public string ErrorMessage { get; }

        public FileUploadFailedEvent(LogFilePath filePath, string errorMessage)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));
            
            FilePath = filePath;
            ErrorMessage = errorMessage;
        }
    }
}