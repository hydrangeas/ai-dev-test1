using System.Threading.Tasks;
using AiDevTest1.Domain.Interfaces;

namespace AiDevTest1.Application.Interfaces
{
    public interface IEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent);
    }
}