using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IAsyncCommand<out TKey>//neden out ?
    {
        TKey Key { get; }
    }
}
