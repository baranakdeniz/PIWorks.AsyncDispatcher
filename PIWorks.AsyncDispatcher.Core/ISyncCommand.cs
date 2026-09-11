using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public interface ISyncCommand<TKey> : IAsyncCommand<TKey>
    {

    }
}
