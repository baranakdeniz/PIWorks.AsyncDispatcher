using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    internal class DefaultAsyncCommandDispatcher<TKey> : IAsyncCommandDispatcher<TKey>
    {
        private readonly ICommandBus<TKey> _commandBus;
        private readonly ICommandTracker<TKey> _commandTracker;
        private readonly ICommandEventPublisher _eventPublisher;

        public DefaultAsyncCommandDispatcher(ICommandBus<TKey> commandBus, ICommandTracker<TKey> commandTracker, ICommandEventPublisher eventPublisher)
        {
            _commandBus = commandBus;
            _commandTracker = commandTracker;
            _eventPublisher = eventPublisher;
        }  
        public virtual async Task EnqueueAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : IAsyncCommand<TKey>
        {
            var envelope = new CommandEnvelope<TCommand, TKey>(command);
           await _commandTracker.UpdateStatusAsync(command.Key, CommandStatus.Pending);
            await _commandBus.EnqueueAsync(envelope, cancellationToken);
        }

        public virtual async Task CancelAsync(TKey commandId, CancellationToken cancellationToken = default)
        {
           await _commandTracker.UpdateStatusAsync(commandId, CommandStatus.Cancelling);
            await _eventPublisher.PublishCancelEventAsync(commandId, cancellationToken);
        }

    
    }
}
