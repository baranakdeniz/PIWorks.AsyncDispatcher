using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ICommandDispatcher<TKey>
    {
        //synchron request
        Task Execute<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ISyncCommand<TKey>;

        //async request
        Task<TKey> EnqueueAsync<TCommand>(TCommand command) where TCommand : IAsyncCommand<TKey>;


       
    }
}
