using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IAsyncCommand<out TKey>
    {
        TKey Key { get; }
    }
}
