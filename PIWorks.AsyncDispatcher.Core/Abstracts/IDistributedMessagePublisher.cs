using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IDistributedMessagePublisher<TKey>
    {
        Task SendCancelCommandRequestAsync(TKey commandId, string targetWorkerId, CancellationToken cancellationToken = default);
    }
}
