
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ICommandCancellationManager<TKey> _commandCancellationManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultAsyncCommandDispatcher<TKey>> _logger;
        private readonly IDistributedMessagePublisher<TKey> _messagePublisher;


        public DefaultAsyncCommandDispatcher(ICommandBus<TKey> commandBus,
            ICommandTracker<TKey> commandTracker,
            ICommandEventPublisher eventPublisher,
            ILogger<DefaultAsyncCommandDispatcher<TKey>> logger, 
            ICommandCancellationManager<TKey> commandCancellationManager,
            IServiceProvider serviceProvider,
            IDistributedMessagePublisher<TKey> messagePublisher
            )
        {
            _commandBus = commandBus;
            _commandTracker = commandTracker;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _commandCancellationManager = commandCancellationManager;
            _serviceProvider = serviceProvider;
            _messagePublisher = messagePublisher;
        }
        public virtual async Task EnqueueAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : IAsyncCommand<TKey>
        {
            var envelope = new CommandEnvelope<TCommand, TKey>(command);
             _commandCancellationManager.RegisterCommand(envelope.CommandId);
            //burada register etmek lazım cancellationtoken durumu için!!!!!!!
            await _eventPublisher.PublishAsync(new CommandPendingEvent<TKey>(command.Key), cancellationToken);
            await _commandBus.EnqueueAsync(envelope, cancellationToken);
            
        }
        //Senkron için!!!!
          public virtual Task<TResult> SendAsync<TCommand ,TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ISyncCommand<TResult>
        { var handler = _serviceProvider.GetRequiredService<ISyncCommandHandler<TCommand, TResult>>();
           return handler.HandleAsync(command, cancellationToken);
 
        }

            public virtual async Task CancelAsync(TKey commandId, CancellationToken cancellationToken = default)
        {// 1. ADIM: Önce bu makinede mi çalışıyor diye bak.
         // Eğer iş bu makinedeyse lokalde CancellationToken'ı patlat ve hemen dön.
            if (_commandCancellationManager.Cancel(commandId))
            {
                _logger.LogInformation("Command with ID {CommandId} was running locally and cancelled.", commandId);
                return;
            }

            // 2. ADIM: Bu makinede değilse Tracker'dan kontrol et (Senin orijinal kodun)
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

            if (string.IsNullOrEmpty(status.Value.WorkerId))
            {
                _logger.LogWarning("WorkerId for Command with ID {CommandId} is unknown.", commandId);
                throw new InvalidOperationException("Komutun hangi makinede olduğu henüz bilinemiyor.");
            }

            // 3. ADIM: Hedef makineyi bulduk! Ağ üzerinden iptal emri fırlatıyoruz.
            // (RabbitMQ'yu doğrudan vermiyoruz, saf bir publisher arayüzü çağırıyoruz)
            await _messagePublisher.SendCancelCommandRequestAsync(commandId, status.Value.WorkerId, cancellationToken);

            _logger.LogInformation("Cancel request for Command {CommandId} sent to Worker {WorkerId}.", commandId, status.Value.WorkerId);
        }
    }

      
    }

    
    

