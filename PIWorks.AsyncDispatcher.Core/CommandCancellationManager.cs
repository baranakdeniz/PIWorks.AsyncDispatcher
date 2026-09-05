using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CommandCancellationManager<TKey> : ICommandCancellationManager<TKey>
    {
        private readonly ConcurrentDictionary<TKey, CancellationTokenSource> _stores = new();

        public CancellationToken RegisterCommand(TKey commandId)
        {
            //var cts = new CancellationTokenSource();
            //_stores.TryAdd(commandId, cts);
            //return cts.Token; 
            var cts = _stores.GetOrAdd(commandId, _ => new CancellationTokenSource());
            return cts.Token;
        }

        public void Remove(TKey commandId)
        {
            if (_stores.TryRemove(commandId, out var cts))
            {
                cts.Dispose(); 
            }
        }

        public bool Cancel(TKey commandId)
        {
            if (_stores.TryGetValue(commandId, out var cts))
            {
                cts.Cancel(); 
                return true;
            }
            return false;
        }
    }
}