using EventBus.Core.Events;

namespace EventBus.Core.Abstractions;

public interface IEventHandler;

public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
