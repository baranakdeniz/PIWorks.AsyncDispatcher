using MediatR;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CancelCommandRequestedEventConsumer<TKey> : INotificationHandler<CancelCommandRequestedEvent<TKey>>
    {
        private readonly ICommandCancellationManager<TKey> _commandCancellationManager;
        private readonly ICommandTracker<TKey> _commandTracker;
        private readonly string _currentAppName = "ProductA";
        public CancelCommandRequestedEventConsumer(ICommandCancellationManager<TKey> commandCancellationManager, ICommandTracker<TKey> commandTracker)
        {
            _commandCancellationManager = commandCancellationManager;
            _commandTracker = commandTracker;
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
