using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Connection;

internal interface IRabbitMqConnection : IDisposable, IAsyncDisposable
{
    bool IsConnected { get; }

    Task<IChannel> CreateChannelAsync(
        CreateChannelOptions? options = null,
        CancellationToken cancellationToken = default);
}
