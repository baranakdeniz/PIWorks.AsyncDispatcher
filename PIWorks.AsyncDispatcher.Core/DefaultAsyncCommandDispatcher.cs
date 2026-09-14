using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using PIWorks.AsyncDispatcher.Core.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class DefaultAsyncCommandDispatcher<TKey> : IAsyncCommandDispatcher<TKey>
    {
        private readonly ICommandBus<TKey> _commandBus;
        private readonly ICommandTracker<TKey> _commandTracker;
        private readonly ICommandEventPublisher _eventPublisher;
        private readonly string _appName;
        private readonly ILogger<DefaultAsyncCommandDispatcher<TKey>> _logger;
      

        public DefaultAsyncCommandDispatcher(ICommandBus<TKey> commandBus, ICommandTracker<TKey> commandTracker, ICommandEventPublisher eventPublisher, ILogger<DefaultAsyncCommandDispatcher<TKey>> logger,IOptions<AsyncDispatcherOptions> options)
        {
            _commandBus = commandBus;
            _commandTracker = commandTracker;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _appName = options.Value.AppName;
        }  
        public virtual async Task EnqueueAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : IAsyncCommand<TKey>
        {
            var envelope = new CommandEnvelope<TCommand, TKey>(command);
            await _eventPublisher.PublishAsync(new CommandPendingEvent<TKey>(command.Key, _appName), cancellationToken);
            await _commandBus.EnqueueAsync(envelope, cancellationToken);
        }
    
       
  
            public virtual async Task CancelAsync(TKey commandId, CancellationToken cancellationToken = default)
        {
            var status = await _commandTracker.GetStatusAsync(commandId);
            if (status == null)
            {
                _logger.LogWarning("Command with ID {CommandId} not found.", commandId);
                throw new KeyNotFoundException($"Command with ID {commandId} not found.");
            }

            if (status.Value.Status == CommandStatus.Finished || status.Value.Status == CommandStatus.Cancelled)
            {
                _logger.LogInformation("This Command is already Completed or Cancelled: {CommandId}", commandId);
                throw new InvalidOperationException($"Command with ID {commandId} is already {status.Value.Status}.");
            }

          
            var cancelRequestedEvent = new CancelCommandRequestedEvent<TKey>(commandId, _appName);

            await _eventPublisher.PublishAsync(cancelRequestedEvent, cancellationToken);
        }

    }

    
    
}
