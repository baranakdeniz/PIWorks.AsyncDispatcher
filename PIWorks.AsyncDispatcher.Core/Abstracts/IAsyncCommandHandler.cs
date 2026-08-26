using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IAsyncCommandHandler<in TCommand ,TKey>
        where TCommand :IAsyncCommand<TKey> 
    {
        Task ExecuteAsync(TCommand command, CancellationToken cancellationToken);

}
}
