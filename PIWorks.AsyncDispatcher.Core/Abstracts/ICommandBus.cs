using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ICommandBus<TKey>
    {
         Task EnqueueAsync(CommandEnvelope<TKey> envelope,CancellationToken cancellationToken = default);

        Task<CommandEnvelope<TKey>> DequeueAsync(CancellationToken cancellationToken);
    }
}
