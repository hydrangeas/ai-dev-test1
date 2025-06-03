using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiDevTest1.Infrastructure.Events
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventDispatcher> _logger;
        private readonly ConcurrentDictionary<Type, MethodInfo> _methodCache = new();

        public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            var eventType = domainEvent.GetType();
            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            
            using var scope = _serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices(handlerType);

            var tasks = new List<Task>();
            foreach (var handler in handlers)
            {
                if (handler == null) continue;

                var handleMethod = GetOrCacheHandleMethod(handlerType);
                if (handleMethod != null)
                {
                    tasks.Add(InvokeHandlerAsync(handler, handleMethod, domainEvent, cancellationToken));
                }
            }

            if (!tasks.Any())
            {
                _logger.LogWarning("No handlers found for event type {EventType}", eventType.Name);
                return;
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "One or more handlers failed while processing event {EventType}", eventType.Name);
                throw new AggregateException($"One or more handlers failed while processing {eventType.Name}", ex);
            }
        }

        private MethodInfo GetOrCacheHandleMethod(Type handlerType)
        {
            return _methodCache.GetOrAdd(handlerType, type =>
            {
                var method = type.GetMethod("HandleAsync");
                if (method == null)
                {
                    _logger.LogError("HandleAsync method not found on handler type {HandlerType}", type);
                }
                return method;
            });
        }

        private async Task InvokeHandlerAsync(object handler, MethodInfo handleMethod, IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            try
            {
                var task = (Task)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken });
                if (task != null)
                {
                    await task;
                }
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                _logger.LogError(tie.InnerException, "Handler {HandlerType} failed to process event {EventType}", 
                    handler.GetType().Name, domainEvent.GetType().Name);
                throw tie.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler {HandlerType} failed to process event {EventType}", 
                    handler.GetType().Name, domainEvent.GetType().Name);
                throw;
            }
        }
    }
}