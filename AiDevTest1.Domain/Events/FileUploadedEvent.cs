using AiDevTest1.Domain.ValueObjects;

namespace AiDevTest1.Domain.Events
{
    public class FileUploadedEvent : DomainEventBase
    {
        public LogFilePath FilePath { get; }
        public BlobName BlobName { get; }
        public string BlobUri { get; }

        public FileUploadedEvent(LogFilePath filePath, BlobName blobName, string blobUri)
        {
            FilePath = filePath;
            BlobName = blobName;
            BlobUri = blobUri;
        }
    }
}