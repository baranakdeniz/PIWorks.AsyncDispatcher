using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ISyncCommand<TKey>
    {
        public TKey Key { get; }
    }
}
