using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ICommandCancellationManager<TKey>
    {
        CancellationToken RegisterCommand(TKey commandId);

        CancellationToken GetToken(TKey commandId);
        void Remove(TKey commandId);
        

        bool Cancel(TKey commandId);
    }
}
