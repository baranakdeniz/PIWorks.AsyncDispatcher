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
        public virtual async Task<TKey> EnqueueAsync<TCommand>(TCommand command) where TCommand : IAsyncCommand<TKey>
        { 
            var envelope = new CommandEnvelope<TCommand, TKey>
            {
                CommandId = command.Key,
                Command = command
            };
           await _commandBus.EnqueueAsync(envelope);
            return command.Key;//inşAllah AHmet abinin dediği yapı budur sor ona?

        }

        public async Task Execute<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ISyncCommand<TKey>
        {
           using var scope =  _scopeFactory.CreateScope();

            var handler = scope.ServiceProvider.GetRequiredService<ISyncCommandHandler<TCommand, TKey>>();

           await  handler.Execute(command, cancellationToken);
          

        }
    }
}
