using Microsoft.Extensions.DependencyInjection;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIWorks.AsyncDispatcher.Core
{
    public class InternalEventPublisher : IInternalEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        public InternalEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDispatcherEvent
        {// O anki scope'dan bu event'i dinleyen tüm handler'ları bulma işi
            var handlers = _serviceProvider.GetServices<IDispatcherEventHandler<TEvent>>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event, cancellationToken);
            }

        }
    }
}
