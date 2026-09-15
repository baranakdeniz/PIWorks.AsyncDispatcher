using MediatR;
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
        INotificationHandler<CommandPendingEvent<TKey>>,
        INotificationHandler<CommandRunningEvent<TKey>>,
        INotificationHandler<CommandFinishedEvent<TKey>>,
        INotificationHandler<CommandErrorEvent<TKey>>,
        INotificationHandler<CommandCancelledEvent<TKey>>
    {
        private readonly ICommandTracker<TKey> _tracker;
      

        public CommandStateEventConsumer(ICommandTracker<TKey> tracker)
        {
            _tracker = tracker;
           
        }

        public async Task Handle(CommandCancelledEvent<TKey> notification, CancellationToken cancellationToken)
        {
              await  _tracker.UpdateStatusAsync(notification.CommandId, CommandStatus.Cancelled, notification.WorkerId);
        }

        public async Task Handle(CommandRunningEvent<TKey> notification, CancellationToken cancellationToken)
        {
           
                await _tracker.UpdateStatusAsync(notification.CommandId, CommandStatus.Running, notification.WorkerId);
        }

        public async Task Handle(CommandErrorEvent<TKey> notification, CancellationToken cancellationToken)
        {
           
                await _tracker.UpdateStatusAsync(notification.CommandId, CommandStatus.Error, notification.WorkerId, notification.ErrorMessage);
        }

        public async Task Handle(CommandFinishedEvent<TKey> notification, CancellationToken cancellationToken)
        {
           
                await _tracker.UpdateStatusAsync(notification.CommandId, CommandStatus.Finished, notification.WorkerId);
        }

        public async Task Handle(CommandPendingEvent<TKey> notification, CancellationToken cancellationToken)
        {
           
                await _tracker.InitializeAsync(notification.CommandId);
        }
    }
}
