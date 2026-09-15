using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IAsyncCommandDispatcher<TKey>
    {
        Task EnqueueAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : IAsyncCommand<TKey>;

        Task<TResult> SendAsync<TCommand , TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand :ISyncCommand<TResult>;

        Task CancelAsync(TKey commandId, CancellationToken cancellationToken = default) ;
    }
}
