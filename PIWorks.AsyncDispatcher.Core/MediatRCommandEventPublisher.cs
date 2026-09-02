using MediatR;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class MediatRCommandEventPublisher<TKey> : ICommandEventPublisher
    {
        //burada benim publish(broadcast) yapmam lazım ki consumerlar dinleyebilsin. 
        private readonly IPublisher _mediator;

        public MediatRCommandEventPublisher(IPublisher mediator)
        {
            _mediator = mediator;
        }
        public Task PublishCancelAsync<TKey>(TKey commandId , CancellationToken cancellationToken = default)
        {
           return _mediator.Publish(new CommandCancelledEvent<TKey>(commandId) , cancellationToken);
          
        }
    }
}
