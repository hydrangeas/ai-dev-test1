using System;

namespace AiDevTest1.Domain.Interfaces
{
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
        Guid EventId { get; }
    }
}