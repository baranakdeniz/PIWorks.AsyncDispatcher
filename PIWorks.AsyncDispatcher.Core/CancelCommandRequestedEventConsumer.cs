using Microsoft.Extensions.Options;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using PIWorks.AsyncDispatcher.Core.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CancelCommandRequestedEventConsumer<TKey> : IDispatcherEventHandler<CancelCommandRequestedEvent<TKey>>
    {
        private readonly ICommandCancellationManager<TKey> _commandCancellationManager;
        private readonly ICommandTracker<TKey> _commandTracker;
    
        public CancelCommandRequestedEventConsumer(ICommandCancellationManager<TKey> commandCancellationManager, ICommandTracker<TKey> commandTracker)
        {
            _commandCancellationManager = commandCancellationManager;
            _commandTracker = commandTracker;
           
           
        }

        public async Task HandleAsync(CancelCommandRequestedEvent<TKey> @event, CancellationToken cancellationToken = default)
        {
             await _commandTracker.UpdateStatusAsync(@event.CommandId, CommandStatus.Cancelling,null);
          
            _commandCancellationManager.Cancel(@event.CommandId);
        }
    }
}
