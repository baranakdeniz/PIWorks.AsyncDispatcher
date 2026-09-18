using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IInternalEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event , CancellationToken cancellationToken=default) where TEvent : IDispatcherEvent;
    }
}
