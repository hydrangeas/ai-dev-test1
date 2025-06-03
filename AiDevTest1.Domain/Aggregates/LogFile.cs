using System;
using System.Collections.Generic;
using System.Linq;
using AiDevTest1.Domain.Events;
using AiDevTest1.Domain.Exceptions;
using AiDevTest1.Domain.Interfaces;
using AiDevTest1.Domain.Models;
using AiDevTest1.Domain.ValueObjects;

namespace AiDevTest1.Domain.Aggregates
{
    public class LogFile
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        private readonly LogFilePath _filePath;
        private readonly List<LogEntry> _entries;
        private readonly DateTime _date;

        public LogFile(DateTime date)
        {
            _date = date.Date;
            _filePath = new LogFilePath($"{_date:yyyy-MM-dd}.log");
            _entries = new List<LogEntry>();
        }

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        public LogFilePath FilePath => _filePath;
        public DateTime Date => _date;
        public IReadOnlyList<LogEntry> Entries => _entries.AsReadOnly();

        public void AddEntry(LogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            // 日付の検証 - JSTで比較
            var entryDateJst = entry.Timestamp.ToLocalTime().Date;
            if (entryDateJst != _date)
            {
                throw new DomainException($"ログエントリの日付 ({entryDateJst:yyyy-MM-dd}) がファイルの日付 ({_date:yyyy-MM-dd}) と一致しません");
            }

            _entries.Add(entry);
            _domainEvents.Add(new LogWrittenToFileEvent(_filePath, entry));
        }

        public string GetContent()
        {
            return string.Join(Environment.NewLine, _entries.Select(e => e.ToJsonLine()));
        }

        public void ClearEvents()
        {
            _domainEvents.Clear();
        }
    }
}