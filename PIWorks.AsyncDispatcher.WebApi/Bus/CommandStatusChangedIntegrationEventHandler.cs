using EventBus.Core.Abstractions;
using PIWorks.AsyncDispatcher.Core;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System.Net.NetworkInformation;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public class CommandStatusChangedIntegrationEventHandler : IEventHandler<CommandStatusChangedIntegrationEvent>
    {  private readonly ICommandTracker<Guid> _tracker;
        public CommandStatusChangedIntegrationEventHandler(ICommandTracker<Guid> tracker)
        {
            _tracker = tracker;
        }
        public async Task HandleAsync(CommandStatusChangedIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            var status = Enum.Parse<CommandStatus>(@event.Status);

            await _tracker.UpdateStatusAsync(
         @event.CommandId,
         status,
         @event.WorkerId,
         @event.ErrorMessage
     );


        }
    }
}
    

