using System;
using System.Linq;
using AiDevTest1.Domain.Aggregates;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.Exceptions;
using AiDevTest1.Domain.Models;
using FluentAssertions;
using Xunit;

namespace AiDevTest1.Tests.Domain.Aggregates
{
    public class LogFileTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var date = new DateTime(2024, 1, 15, 10, 30, 0);

            // Act
            var logFile = new LogFile(date);

            // Assert
            logFile.Date.Should().Be(date.Date);
            logFile.FilePath.FileName.Should().Be("2024-01-15.log");
            logFile.Entries.Should().BeEmpty();
            logFile.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void AddEntry_ShouldAddEntryAndRaiseEvent()
        {
            // Arrange
            var date = DateTime.Today;
            var logFile = new LogFile(date);
            var entry = new LogEntry(EventType.START);

            // Act
            logFile.AddEntry(entry);

            // Assert
            logFile.Entries.Should().HaveCount(1);
            logFile.Entries.First().Should().Be(entry);
            
            logFile.DomainEvents.Should().HaveCount(1);
            var domainEvent = logFile.DomainEvents.First() as LogWrittenToFileEvent;
            domainEvent.Should().NotBeNull();
            domainEvent.FilePath.Should().Be(logFile.FilePath);
            domainEvent.LogEntry.Should().Be(entry);
        }

        [Fact]
        public void AddEntry_ShouldThrowArgumentNullException_WhenEntryIsNull()
        {
            // Arrange
            var logFile = new LogFile(DateTime.Today);

            // Act
            Action act = () => logFile.AddEntry(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("entry");
        }

        [Fact]
        public void AddEntry_ShouldThrowDomainException_WhenEntryDateDoesNotMatch()
        {
            // Arrange
            var logFileDate = new DateTime(2024, 1, 15);
            var logFile = new LogFile(logFileDate);
            
            // Create an entry with a different date (in UTC)
            var differentDate = new DateTime(2024, 1, 16, 15, 0, 0, DateTimeKind.Utc); // This is 2024-01-17 in JST
            var entry = new LogEntry(EventType.START, differentDate);

            // Act
            Action act = () => logFile.AddEntry(entry);

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("*2024-01-17*2024-01-15*");
        }

        [Fact]
        public void AddEntry_ShouldAcceptEntry_WhenDateMatchesInJST()
        {
            // Arrange
            var logFileDate = new DateTime(2024, 1, 15);
            var logFile = new LogFile(logFileDate);
            
            // Create an entry at 15:00 UTC on 2024-01-14, which is 00:00 JST on 2024-01-15
            var utcDate = new DateTime(2024, 1, 14, 15, 0, 0, DateTimeKind.Utc);
            var entry = new LogEntry(EventType.START, utcDate);

            // Act
            Action act = () => logFile.AddEntry(entry);

            // Assert
            act.Should().NotThrow();
            logFile.Entries.Should().HaveCount(1);
        }

        [Fact]
        public void GetContent_ShouldReturnJsonLines()
        {
            // Arrange
            var logFile = new LogFile(DateTime.Today);
            var entry1 = new LogEntry(EventType.START);
            var entry2 = new LogEntry(EventType.STOP);
            
            logFile.AddEntry(entry1);
            logFile.AddEntry(entry2);

            // Act
            var content = logFile.GetContent();

            // Assert
            var lines = content.Split(Environment.NewLine);
            lines.Should().HaveCount(2);
            lines[0].Should().Be(entry1.ToJsonLine());
            lines[1].Should().Be(entry2.ToJsonLine());
        }

        [Fact]
        public void GetContent_ShouldReturnEmptyString_WhenNoEntries()
        {
            // Arrange
            var logFile = new LogFile(DateTime.Today);

            // Act
            var content = logFile.GetContent();

            // Assert
            content.Should().BeEmpty();
        }

        [Fact]
        public void ClearEvents_ShouldRemoveAllDomainEvents()
        {
            // Arrange
            var logFile = new LogFile(DateTime.Today);
            var entry = new LogEntry(EventType.START);
            logFile.AddEntry(entry);

            // Act
            logFile.ClearEvents();

            // Assert
            logFile.DomainEvents.Should().BeEmpty();
            logFile.Entries.Should().HaveCount(1); // Entries should remain
        }

        [Fact]
        public void AddEntry_ShouldAccumulateMultipleEvents()
        {
            // Arrange
            var logFile = new LogFile(DateTime.Today);
            var entry1 = new LogEntry(EventType.START);
            var entry2 = new LogEntry(EventType.WARN);
            var entry3 = new LogEntry(EventType.ERROR);

            // Act
            logFile.AddEntry(entry1);
            logFile.AddEntry(entry2);
            logFile.AddEntry(entry3);

            // Assert
            logFile.Entries.Should().HaveCount(3);
            logFile.DomainEvents.Should().HaveCount(3);
            logFile.DomainEvents.Should().AllBeOfType<LogWrittenToFileEvent>();
        }
    }
}