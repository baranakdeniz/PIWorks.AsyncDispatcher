using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IDispatcherEventHandler<in TEvent> where TEvent : IDispatcherEvent
    {
        Task HandleAsync(TEvent @event , CancellationToken cancellationToken = default);
    }
}
