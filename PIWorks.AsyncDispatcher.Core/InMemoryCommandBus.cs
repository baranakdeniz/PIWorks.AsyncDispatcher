//using Microsoft.Extensions.Options;
//using PIWorks.AsyncDispatcher.Core.Abstracts;
//using PIWorks.AsyncDispatcher.Core.Options;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Channels;

//namespace PIWorks.AsyncDispatcher.Core
//{
//    public class InMemoryCommandBus<TKey> : ICommandBus<TKey>
//    {
//        private readonly Channel<CommandEnvelope<TKey>> _channel;
//        private readonly string _queueName;
//        public InMemoryCommandBus(IOptions<AsyncDispatcherOptions> options)
//        {
//            _channel = Channel.CreateUnbounded<CommandEnvelope<TKey>>();
//            _queueName = $"piworks.commands.{options.Value.AppName}" ;
//        }
        
//        public async Task EnqueueAsync(CommandEnvelope<TKey> envelope, CancellationToken cancellationToken = default)
//        {
//              await _channel.Writer.WriteAsync(envelope,cancellationToken);
//        }

//        public async Task<CommandEnvelope<TKey>> DequeueAsync(CancellationToken cancellationToken)
//        {
//            return await _channel.Reader.ReadAsync(cancellationToken);
//        }

    
//    }
//}
