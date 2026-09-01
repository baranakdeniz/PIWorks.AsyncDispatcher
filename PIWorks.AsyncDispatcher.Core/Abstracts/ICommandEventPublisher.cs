using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ICommandEventPublisher
    {
        Task PublishCancelEventAsync<TKey>(TKey commandId, CancellationToken cancellationToken = default);
    }
}
