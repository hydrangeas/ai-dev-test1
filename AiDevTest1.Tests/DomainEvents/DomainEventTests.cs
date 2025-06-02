using System;
using System.Threading;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AiDevTest1.Tests.DomainEvents
{
    public class DomainEventTests
    {
        [Fact]
        public void DomainEventBase_ShouldSetEventIdAndOccurredAt()
        {
            var @event = new TestDomainEvent();

            @event.EventId.Should().NotBeEmpty();
            @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void LogWrittenToFileEvent_ShouldSetPropertiesCorrectly()
        {
            var filePath = LogFilePath.Create("/test/path.log").Value;
            var logEntry = new LogEntry(EventType.START);

            var @event = new LogWrittenToFileEvent(filePath, logEntry);

            @event.FilePath.Should().Be(filePath);
            @event.LogEntry.Should().Be(logEntry);
            @event.EventId.Should().NotBeEmpty();
            @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void FileUploadedEvent_ShouldSetPropertiesCorrectly()
        {
            var filePath = LogFilePath.Create("/test/path.log").Value;
            var blobName = BlobName.Create("test-blob").Value;
            var blobUri = "https://test.blob.core.windows.net/test";

            var @event = new FileUploadedEvent(filePath, blobName, blobUri);

            @event.FilePath.Should().Be(filePath);
            @event.BlobName.Should().Be(blobName);
            @event.BlobUri.Should().Be(blobUri);
            @event.EventId.Should().NotBeEmpty();
            @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void FileUploadFailedEvent_ShouldSetPropertiesCorrectly()
        {
            var filePath = LogFilePath.Create("/test/path.log").Value;
            var errorMessage = "Upload failed";

            var @event = new FileUploadFailedEvent(filePath, errorMessage);

            @event.FilePath.Should().Be(filePath);
            @event.ErrorMessage.Should().Be(errorMessage);
            @event.EventId.Should().NotBeEmpty();
            @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void MultipleDomainEvents_ShouldHaveUniqueEventIds()
        {
            var event1 = new TestDomainEvent();
            Thread.Sleep(10); // 確実に異なるEventIdを生成するため
            var event2 = new TestDomainEvent();

            event1.EventId.Should().NotBe(event2.EventId);
        }

        [Fact]
        public void FileUploadFailedEvent_ShouldThrowArgumentNullException_WhenFilePathIsNull()
        {
            LogFilePath filePath = null;
            var errorMessage = "Upload failed";

            var action = () => new FileUploadFailedEvent(filePath, errorMessage);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("filePath");
        }

        [Fact]
        public void FileUploadFailedEvent_ShouldThrowArgumentNullException_WhenErrorMessageIsNull()
        {
            var filePath = LogFilePath.Create("/test/path.log").Value;
            string errorMessage = null;

            var action = () => new FileUploadFailedEvent(filePath, errorMessage);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("errorMessage");
        }

        private class TestDomainEvent : DomainEventBase
        {
        }
    }
}