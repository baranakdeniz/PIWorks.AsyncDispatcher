using EventBus.Core.Abstractions;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System.Text.Json;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public class RabbitMqCommandEventPublisher : ICommandEventPublisher//bunun gibi iptali de yapmam lazım iptal eventi!!!
    {
        private readonly IEventBus _eventBus;

        public RabbitMqCommandEventPublisher(IEventBus eventBus)
        {
         _eventBus = eventBus;   
        }
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
        {
            var wrappedEvent = new WrappedIntegrationEvent
            {
                EventTypeName = @event.GetType().AssemblyQualifiedName!,
                EventPayloadJson = JsonSerializer.Serialize(@event)

            };

           await _eventBus.PublishAsync(wrappedEvent, cancellationToken);


        }
    }
}
