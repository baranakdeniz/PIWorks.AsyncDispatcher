using EventBus.Core.Events;

namespace EventBus.Core.Abstractions;
public interface IReplayable
{

    Task ReplayAsync<TEvent>(
        DateTimeOffset from,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
