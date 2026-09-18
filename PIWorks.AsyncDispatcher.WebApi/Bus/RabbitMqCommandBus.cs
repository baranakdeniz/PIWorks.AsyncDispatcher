using EventBus.Core.Abstractions;
using Microsoft.Extensions.Options;
using PIWorks.AsyncDispatcher.Core;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Options;
using System.Text.Json;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public class RabbitMqCommandBus<TKey> : ICommandBus<TKey>
    {
        private readonly IEventBus _eventBus;
      

        public RabbitMqCommandBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
          
        }

        public async Task EnqueueAsync(CommandEnvelope<TKey> envelope, CancellationToken cancellationToken = default)
        {
            var commandProp = envelope.GetType().GetProperty("Command");
            var commandObj = commandProp?.GetValue(envelope);

            var integrationEvent = new CommandIntegrationEvent
            {
                CommandTypeName = commandObj!.GetType().AssemblyQualifiedName!,
                CommandPayloadJson = JsonSerializer.Serialize(commandObj)
            };

            await _eventBus.PublishAsync(integrationEvent, cancellationToken);
        }
        public Task<CommandEnvelope<TKey>> DequeueAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("RabbitMQ push-based çalışır; DequeueAsync desteklenmez.");
        }
    }
}
