using MediatR;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CommandCancellationEventConsumer<TKey> : INotificationHandler<CommandCancelledEvent<TKey>>
    {//bana bu consumer da cancellatonmanager lazım olacak ama niye ? çünkü biz burda bunu tetikleyerek asnycommandworkerda o işin iptalini sağlyacağız diye düşünüyorum.
        private readonly ICommandCancellationManager<TKey> _commandCancellationManager;

        public CommandCancellationEventConsumer(ICommandCancellationManager<TKey> commandCancellationManager)
        {
            _commandCancellationManager = commandCancellationManager;
        }
        public Task Handle(CommandCancelledEvent<TKey> notification, CancellationToken cancellationToken)
        {
           _commandCancellationManager.Cancel(notification.commandId);
            return Task.CompletedTask;
        }
    }
}
