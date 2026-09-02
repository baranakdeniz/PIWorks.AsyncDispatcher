using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ICommandEventPublisher //class bazlı generic kullanırsam kendimi kısıtlarım metod bazlı yaparsam herkes kendine göre!
    {
        Task PublishCancelAsync<TKey>(TKey commandId, CancellationToken cancellationToken = default);

    }
}
