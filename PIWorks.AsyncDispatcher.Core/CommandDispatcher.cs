using Microsoft.Extensions.DependencyInjection;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class CommandDispatcher<TKey> : ICommandDispatcher<TKey>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICommandBus<TKey> _commandBus;
        public CommandDispatcher(IServiceScopeFactory scopeFactory, ICommandBus<TKey> commandBus)
        {
            _scopeFactory = scopeFactory;
            _commandBus = commandBus;
        }
        public async Task<TKey> EnqueueAsync<TCommand>(TCommand command, TKey trackingNumber) where TCommand : IAsyncCommand<TKey>
        { 
            var envelope = new CommandEnvelope<TCommand, TKey>
            {
              Command = command
            };
           await _commandBus.EnqueueAsync(envelope);
            return trackingNumber;//inşAllah AHmet abinin dediği yapı budur sor ona?

        }

        public async Task Execute<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ISyncCommand<TKey>
        {
           using var scope =  _scopeFactory.CreateScope();

            var handler = scope.ServiceProvider.GetRequiredService<ISyncCommandHandler<TCommand, TKey>>();

           await  handler.Execute(command, cancellationToken);
            //scope.Dispose(); koymam lazım mı ? otomatik siler mi ? 

        }
    }
}
