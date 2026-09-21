using EventBus.Core.Abstractions;
using PIWorks.AsyncDispatcher.Core.Abstracts;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public class RabbitMqMessagePublisherAdapter : IDistributedMessagePublisher<Guid>
    {
        private readonly IEventBus _eventBus;

        public RabbitMqMessagePublisherAdapter(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task SendCancelCommandRequestAsync(Guid commandId, string targetWorkerId, CancellationToken cancellationToken = default)
        {
            // Core'dan gelen emri, Mücahit'in Event'ine dönüştürüp RabbitMQ'ya basıyorum !!!!
            await _eventBus.PublishAsync(new CancelCommandRequestedIntegrationEvent
            {
                CommandId = commandId,
                TargetWorkerId = targetWorkerId
            }, cancellationToken);
        }
    }
}
