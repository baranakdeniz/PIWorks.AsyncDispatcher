using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    internal interface IAsyncCommandDispatcher<TKey>
    {
        Task EnqueueAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : IAsyncCommand<TKey>;

        Task CancelAsync(TKey commandId, CancellationToken cancellationToken = default) ;
    }
}
