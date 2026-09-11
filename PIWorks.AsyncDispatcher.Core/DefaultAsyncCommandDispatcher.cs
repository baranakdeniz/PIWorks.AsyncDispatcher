using Microsoft.Extensions.Logging;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class DefaultAsyncCommandDispatcher<TKey> : IAsyncCommandDispatcher<TKey>
    {
        private readonly ICommandBus<TKey> _commandBus;
        private readonly ICommandTracker<TKey> _commandTracker;
        private readonly ICommandEventPublisher<TKey> _eventPublisher;
        private readonly ILogger<DefaultAsyncCommandDispatcher<TKey>> _logger;
      

        public DefaultAsyncCommandDispatcher(ICommandBus<TKey> commandBus, ICommandTracker<TKey> commandTracker, ICommandEventPublisher<TKey> eventPublisher, ILogger<DefaultAsyncCommandDispatcher<TKey>> logger)
        {
            _commandBus = commandBus;
            _commandTracker = commandTracker;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }  
        public virtual async Task EnqueueAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : IAsyncCommand<TKey>
        {
         
            var envelope = new CommandEnvelope<TCommand, TKey>(command);
            await _commandTracker.InitializeAsync(command.Key);
            await _commandBus.EnqueueAsync(envelope, cancellationToken);
        }
    
        public virtual async Task CancelAsync(TKey commandId,CancellationToken cancellationToken = default)
        {
            var status = await _commandTracker.GetStatusAsync(commandId);
            if(status== null)
            {
                _logger.LogWarning("Command with ID {CommandId} not found.", commandId);
                throw new KeyNotFoundException($"Command with ID {commandId} not found.");
            }
            //şimdi tamamlanmış bir id yi iptal etmek istese de hata vermeli !
            if (status.Value.Status == CommandStatus.Finished || status.Value.Status == CommandStatus.Cancelled)
            {
                _logger.LogInformation("This Command is already Completed or Cancelled: {CommandId}", commandId);
                return;

            }
            await _commandTracker.UpdateStatusAsync(commandId, CommandStatus.Cancelling, null);
            await _eventPublisher.PublishCancelAsync(commandId, cancellationToken);
        }

    
    }
}
