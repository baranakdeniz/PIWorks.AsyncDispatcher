using System.Text.Json;
using EventBus.Core.Abstractions;
using EventBus.Core.Events;
using EventBus.RabbitMQ.Configuration;
using EventBus.RabbitMQ.Connection;
using EventBus.RabbitMQ.Internal;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ;

internal sealed class RabbitMqEventBus : IEventBus, IDisposable, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IRabbitMqConnection _connection;
    private readonly EventSubscriptionsManager _subscriptionsManager;
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _publishLock = new(1, 1);

    private IChannel? _publisherChannel;
    private bool _disposed;

    public RabbitMqEventBus(
        IRabbitMqConnection connection,
        EventSubscriptionsManager subscriptionsManager,
        IOptions<RabbitMqOptions> options)
    {
        _connection = connection;
        _subscriptionsManager = subscriptionsManager;
        _options = options.Value;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {

        var eventType = @event.GetType();
        var body = JsonSerializer.SerializeToUtf8Bytes(@event, eventType, JsonOptions);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json",
        };

        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetPublisherChannelAsync(cancellationToken);

            await channel.BasicPublishAsync(
                _options.ExchangeName,
                eventType.Name,
                mandatory: false,
                properties,
                body,
                cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    private async Task<IChannel> GetPublisherChannelAsync(CancellationToken cancellationToken)
    {
        if (_publisherChannel is { IsOpen: true })
        {
            return _publisherChannel;
        }

        _publisherChannel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        await _publisherChannel.ExchangeDeclareAsync(
            _options.ExchangeName,
            ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        return _publisherChannel;
    }

    public void Subscribe<THandler>() where THandler : IEventHandler
    {
        _subscriptionsManager.AddSubscription<THandler>();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _publisherChannel?.Dispose();
        _publishLock.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_publisherChannel is not null)
        {
            await _publisherChannel.DisposeAsync();
        }

        _publishLock.Dispose();
    }
}
