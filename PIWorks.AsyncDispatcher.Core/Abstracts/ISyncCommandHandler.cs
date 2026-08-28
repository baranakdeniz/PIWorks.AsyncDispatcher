using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    internal interface ISyncCommandHandler<TCommand, TKey> where TCommand : ISyncCommand<TKey>
    {
        Task Execute(TCommand command, CancellationToken cancellationToken);
    }
}
