using MediatR;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    internal class MediatRCommandEventPublisher : ICommandEventPublisher
    {
        private readonly IPublisher _mediator;
        public MediatRCommandEventPublisher(IPublisher mediator)
        {
            _mediator = mediator;
        }
        public Task PublishCancelEventAsync<TKey>(TKey commandId, CancellationToken cancellationToken = default)
        {
            return _mediator.Publish(new CommandCancelledEvent<TKey>(commandId), cancellationToken);//gösterim nasıl ?
        }
    }
}
