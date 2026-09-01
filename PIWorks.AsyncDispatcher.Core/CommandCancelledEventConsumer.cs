using MediatR;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CommandCancelledEventConsumer<TKey> : INotificationHandler<CommandCancelledEvent<TKey>>
    {
        private readonly ICommandCancellationManager<TKey> _commandCancellationManager;
        public CommandCancelledEventConsumer(ICommandCancellationManager<TKey> commandCancellationManager)
        { 
            _commandCancellationManager = commandCancellationManager;
        }
        public Task Handle(CommandCancelledEvent<TKey> notification, CancellationToken cancellationToken)
        {_commandCancellationManager.Cancel(notification.CommandId);
            return Task.CompletedTask;

        }
    }
}
