using EventBus.Core.Events;

namespace EventBus.Core.Abstractions;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    void Subscribe<THandler>() where THandler :  IEventHandler;
}
