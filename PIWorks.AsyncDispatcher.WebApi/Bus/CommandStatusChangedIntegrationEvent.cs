using EventBus.Core.Events;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public record CommandStatusChangedIntegrationEvent : IntegrationEvent
    {//Durum Değiştiğinde Tüm Sunuculara (Broadcast) Gidecek Haber
        public Guid CommandId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string WorkerId { get; init; } = string.Empty;
        public string? ErrorMessage { get; init; }
    }
}
