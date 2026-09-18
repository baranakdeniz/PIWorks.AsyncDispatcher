using EventBus.Core.Events;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public record CancelCommandRequestedIntegrationEvent : IntegrationEvent
    {
        public Guid CommandId { get; init; }
        public string TargetWorkerId { get; init; } = string.Empty; // Hedef sunucu kim? onu bulucaz.
    }
}
