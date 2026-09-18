using EventBus.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Connection;

internal sealed class RabbitMqConnection : IRabbitMqConnection
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly int _retryCount;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
    {
        var configured = options.Value;
        _connectionFactory = new ConnectionFactory
        {
            HostName = configured.HostName,
            Port = configured.Port,
            UserName = configured.UserName,
            Password = configured.Password,
            VirtualHost = configured.VirtualHost,
            AutomaticRecoveryEnabled = true,
        };
        _logger = logger;
        _retryCount = configured.RetryCount;
    }

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public async Task<IChannel> CreateChannelAsync(
        CreateChannelOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            await EnsureConnectedAsync(cancellationToken);
        }

        return await _connection!.CreateChannelAsync(options, cancellationToken);
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                return;
            }

            var pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = _retryCount,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(1),
                    OnRetry = args =>
                    {
                        _logger.LogWarning(
                            args.Outcome.Exception,
                            "Could not connect to RabbitMQ, retrying (attempt {Attempt})",
                            args.AttemptNumber + 1);
                        return ValueTask.CompletedTask;
                    },
                })
                .Build();

            _connection = await pipeline.ExecuteAsync(
                async ct => await _connectionFactory.CreateConnectionAsync(ct),
                cancellationToken);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _connection?.Dispose();
        _connectionLock.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}
