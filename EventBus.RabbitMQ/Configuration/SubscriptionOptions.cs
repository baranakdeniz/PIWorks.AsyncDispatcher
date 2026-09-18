using EventBus.Core.Abstractions;

namespace EventBus.RabbitMQ.Configuration;

public sealed class SubscriptionOptions
{
    public SubscriptionMode Mode { get; init; } = SubscriptionMode.Competing;

    public ushort PrefetchCount { get; init; }
}
