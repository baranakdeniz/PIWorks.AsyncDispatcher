using MediatR;
using Microsoft.Extensions.Options;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using PIWorks.AsyncDispatcher.Core.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CancelCommandRequestedEventConsumer<TKey> : INotificationHandler<CancelCommandRequestedEvent<TKey>>
    {
        private readonly ICommandCancellationManager<TKey> _commandCancellationManager;
        private readonly ICommandTracker<TKey> _commandTracker;
        private readonly string _currentAppName;
        public CancelCommandRequestedEventConsumer(ICommandCancellationManager<TKey> commandCancellationManager, ICommandTracker<TKey> commandTracker, IOptions<AsyncDispatcherOptions> options)
        {
            _commandCancellationManager = commandCancellationManager;
            _commandTracker = commandTracker;
            _currentAppName = options.Value.AppName;
           
        }
        public async Task Handle(CancelCommandRequestedEvent<TKey> notification, CancellationToken cancellationToken)
        {
            //gelen mesaj bizim projeye mi ait kontrolü?
            if (notification.AppName != _currentAppName)
            {
                return;
            }
            await _commandTracker.UpdateStatusAsync(notification.CommandId, CommandStatus.Cancelling,null);
          
            _commandCancellationManager.Cancel(notification.CommandId);

        }
    }
}
