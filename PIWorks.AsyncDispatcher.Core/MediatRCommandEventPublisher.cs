using MediatR;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class MediatRCommandEventPublisher : ICommandEventPublisher
    {
        //burada benim publish(broadcast) yapmam lazım ki consumerlar dinleyebilsin. 
        private readonly IPublisher _mediator;

        public MediatRCommandEventPublisher(IPublisher mediator)
        {
            _mediator = mediator;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
        {
            await _mediator.Publish(@event, cancellationToken);
        }
    }
    }

