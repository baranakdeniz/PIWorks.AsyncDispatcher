using EventBus.Core.Events;
using PIWorks.AsyncDispatcher.Core.Abstracts;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public record WrappedIntegrationEvent : IntegrationEvent
    {
        public string EventTypeName { get; init; } = string.Empty;
        public string EventPayloadJson { get; init; } = string.Empty;
    }
}
