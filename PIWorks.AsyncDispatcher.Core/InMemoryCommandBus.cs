using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace PIWorks.AsyncDispatcher.Core
{
    public class InMemoryCommandBus<TKey> : ICommandBus<TKey>
    {
        private readonly Channel<CommandEnvelope<TKey>> _channel;
        public InMemoryCommandBus()
        {
            _channel = Channel.CreateUnbounded<CommandEnvelope<TKey>>();
        }
        public async Task<CommandEnvelope<TKey>> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }

        public async Task EnqueueAsync(CommandEnvelope<TKey> envelope, CancellationToken cancellationToken = default)
        {
              await _channel.Writer.WriteAsync(envelope,cancellationToken);
        }
    }
}
