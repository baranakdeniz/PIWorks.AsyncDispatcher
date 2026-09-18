
using Microsoft.Extensions.Options;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using PIWorks.AsyncDispatcher.Core.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CommandStateEventConsumer<TKey> :
        IDispatcherEventHandler<CommandPendingEvent<TKey>>,
        IDispatcherEventHandler<CommandRunningEvent<TKey>>,
        IDispatcherEventHandler<CommandFinishedEvent<TKey>>,
        IDispatcherEventHandler<CommandErrorEvent<TKey>>,
        IDispatcherEventHandler<CommandCancelledEvent<TKey>>
    {
        private readonly ICommandTracker<TKey> _tracker;
      

        public CommandStateEventConsumer(ICommandTracker<TKey> tracker)
        {
            _tracker = tracker;
           
        }

        public async Task HandleAsync(CommandCancelledEvent<TKey> @event, CancellationToken cancellationToken)
        {
              await  _tracker.UpdateStatusAsync(@event.CommandId, CommandStatus.Cancelled, @event.WorkerId);
        }

        public async Task HandleAsync(CommandRunningEvent<TKey> @event, CancellationToken cancellationToken)
        {
           
                await _tracker.UpdateStatusAsync(@event.CommandId, CommandStatus.Running, @event.WorkerId);
        }

        public async Task HandleAsync(CommandErrorEvent<TKey> @event, CancellationToken cancellationToken)
        {
           
                await _tracker.UpdateStatusAsync(@event.CommandId, CommandStatus.Error, @event.WorkerId, @event.ErrorMessage);
        }

        public async Task HandleAsync(CommandFinishedEvent<TKey> @event, CancellationToken cancellationToken)
        {
           
                await _tracker.UpdateStatusAsync(@event.CommandId, CommandStatus.Finished, @event.WorkerId);
        }

        public async Task HandleAsync(CommandPendingEvent<TKey> @event, CancellationToken cancellationToken)
        {
           
                await _tracker.InitializeAsync(@event.CommandId);
        }
    }
}
