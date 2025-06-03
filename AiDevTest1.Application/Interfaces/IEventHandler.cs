using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Domain.Interfaces;

namespace AiDevTest1.Application.Interfaces
{
    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
    }
}