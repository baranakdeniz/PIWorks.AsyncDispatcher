using EventBus.Core.Events;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public record CommandIntegrationEvent : IntegrationEvent//zarf olarak kullanıyorum
    {
        public string CommandTypeName { get; init; } = string.Empty;
        public string CommandPayloadJson { get; init; } = string.Empty;

    }
}
