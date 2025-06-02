using System;
using AiDevTest1.Domain.Interfaces;

namespace AiDevTest1.Domain.Events
{
    public abstract class DomainEventBase : IDomainEvent
    {
        protected DomainEventBase()
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
        }

        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
    }
}