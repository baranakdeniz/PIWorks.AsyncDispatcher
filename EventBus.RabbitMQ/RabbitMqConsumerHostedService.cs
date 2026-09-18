using System.Text.Json;
using EventBus.Core.Abstractions;
using EventBus.RabbitMQ.Configuration;
using EventBus.RabbitMQ.Connection;
using EventBus.RabbitMQ.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ;

internal sealed class RabbitMqConsumerHostedService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IRabbitMqConnection _connection;
    private readonly EventSubscriptionsManager _subscriptionsManager;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConsumerHostedService> _logger;

    private readonly string _instanceId = Guid.NewGuid().ToString("N")[..8];
    private readonly List<string> _consumerTags = [];

    private int _inFlight;

    private IChannel? _channel;
    private CancellationToken _stoppingToken = CancellationToken.None;

    public RabbitMqConsumerHostedService(
        IRabbitMqConnection connection,
        EventSubscriptionsManager subscriptionsManager,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConsumerHostedService> logger)
    {
        _connection = connection;
        _subscriptionsManager = subscriptionsManager;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var configured in _options.Subscriptions.Keys.Where(key => !_subscriptionsManager.EventNames.Contains(key)))
        {
            _logger.LogWarning(
                "Subscription settings were configured for '{EventName}' but no handler is subscribed to it, "
                + "so the settings are ignored. Check the event name for typos.",
                configured);
        }

        if (_subscriptionsManager.EventNames.Count == 0)
        {
            _logger.LogInformation("No subscriptions registered, RabbitMQ consumer was not started.");
        }
        else
        {
            _channel = await _connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: false,
                    publisherConfirmationTrackingEnabled: false,
                    consumerDispatchConcurrency: _options.MaxConcurrency),
                cancellationToken);

            await _channel.ExchangeDeclareAsync(
                _options.ExchangeName,
                ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken);

            foreach (var eventName in _subscriptionsManager.EventNames)
            {
                var broadcast = SubscriptionFor(eventName).Mode == SubscriptionMode.Broadcast;

                var queueName = QueueNameFor(eventName, broadcast);

                await _channel.QueueDeclareAsync(
                    queueName,
                    durable: !broadcast,
                    exclusive: broadcast,
                    autoDelete: broadcast,
                    cancellationToken: cancellationToken);

                await _channel.QueueBindAsync(
                    queueName,
                    _options.ExchangeName,
                    eventName,
                    cancellationToken: cancellationToken);
            }
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            return;
        }

        _stoppingToken = stoppingToken;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        foreach (var eventName in _subscriptionsManager.EventNames)
        {
            var subscription = SubscriptionFor(eventName);

            if (subscription.PrefetchCount > 0)
            {
                await _channel.BasicQosAsync(
                    prefetchSize: 0,
                    subscription.PrefetchCount,
                    global: false,
                    cancellationToken: stoppingToken);
            }

            var consumerTag = await _channel.BasicConsumeAsync(
                QueueNameFor(eventName, subscription.Mode == SubscriptionMode.Broadcast),
                autoAck: false,
                consumer,
                stoppingToken);

            _consumerTags.Add(consumerTag);
        }

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private string QueueNameFor(string eventName, bool broadcast) =>
        broadcast
            ? $"{_options.ClientName}.{eventName}.{_instanceId}"
            : $"{_options.ClientName}.{eventName}";

    private SubscriptionOptions SubscriptionFor(string eventName) =>
        _options.Subscriptions.GetValueOrDefault(eventName) ?? new SubscriptionOptions();

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;

        Interlocked.Increment(ref _inFlight);
        try
        {
            if (_subscriptionsManager.TryGetEventType(eventName, out var eventType) && eventType is not null)
            {
                var @event = JsonSerializer.Deserialize(eventArgs.Body.Span, eventType, JsonOptions);
                if (@event is not null)
                {
                    await DispatchToHandlersAsync(eventName, eventType, @event);
                }
            }
            else
            {
                _logger.LogWarning(
                    "No subscription registered for '{EventName}', skipping message. A removed "
                    + "subscription may still have its queue binding in place.",
                    eventName);
            }

            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle '{EventName}', dropping the message from the queue", eventName);
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
        }
        finally
        {
            Interlocked.Decrement(ref _inFlight);
        }
    }

    private async Task DispatchToHandlersAsync(string eventName, Type eventType, object @event)
    {
        using var scope = _scopeFactory.CreateScope();

        foreach (var handlerType in _subscriptionsManager.GetHandlerTypes(eventName))
        {
            var handler = scope.ServiceProvider.GetRequiredService(handlerType);
            var handlerInterface = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handleMethod = handlerInterface.GetMethod(nameof(IEventHandler<Core.Events.IntegrationEvent>.HandleAsync))!;

            await (Task)handleMethod.Invoke(handler, [@event, _stoppingToken])!;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            foreach (var consumerTag in _consumerTags)
            {
                await _channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
            }
        }

        await base.StopAsync(cancellationToken);

        while (Volatile.Read(ref _inFlight) > 0 && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(50, CancellationToken.None);
        }

        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
        }
    }
}
