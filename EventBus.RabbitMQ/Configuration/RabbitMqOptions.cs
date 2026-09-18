using System.ComponentModel.DataAnnotations;

namespace EventBus.RabbitMQ.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = "EventBus:RabbitMQ";

    [Required]
    public required string HostName { get; init; }

    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";

    [Required]
    public required string ClientName { get; init; }

    public string ExchangeName { get; init; } = "eventbus";

    public int RetryCount { get; init; } = 5;

    [Range(1, 255)]
    public ushort MaxConcurrency { get; init; } = 1;

    public Dictionary<string, SubscriptionOptions> Subscriptions { get; init; } = [];
}
