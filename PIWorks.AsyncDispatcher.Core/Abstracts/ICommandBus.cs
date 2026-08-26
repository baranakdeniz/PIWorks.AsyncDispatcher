using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ICommandBus<TKey>
    {
        Task EnqueueAsync(CommandEnvelope<TKey> envelope);

        Task<CommandEnvelope<TKey>> DequeueAsync(CancellationToken cancellationToken);
    }
}
