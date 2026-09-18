using Microsoft.Extensions.DependencyInjection;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public abstract class CommandEnvelope<TKey> 
    {//double dispatch 1.dağıtım worker - > envelope
        public TKey CommandId { get; }
        protected CommandEnvelope(TKey commandId)
        {
            CommandId = commandId;
        }

        public abstract Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
        public abstract Task HandleCancellationAsync(IServiceProvider serviceProvider);
        public abstract Task HandleFailureAsync(IServiceProvider serviceProvider, Exception ex);
    } 
    public class CommandEnvelope<TCommand,TKey> : CommandEnvelope<TKey> where TCommand : IAsyncCommand<TKey>
    {//double dispatch 2.dağıtım envelope -> handler
        public TCommand Command { get; }

        public CommandEnvelope(TCommand command) : base(command.Key)
        {
            ArgumentNullException.ThrowIfNull(command);
            Command = command;
        }

        public override async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
           
            var handler = serviceProvider.GetRequiredService<IAsyncCommandHandler<TCommand, TKey>>();
            await handler.ExecuteAsync(Command, cancellationToken);

        }

        public override async Task HandleCancellationAsync(IServiceProvider serviceProvider)
        {
            var handler = serviceProvider.GetRequiredService<IAsyncCommandHandler<TCommand, TKey>>();
            if (handler is IAsyncCommandHandler<TCommand, TKey>.ICancellationHandler cancelHandler)
            {
                await cancelHandler.HandleAsyncOperationCancellation(Command);
            }
        }

        public override async Task HandleFailureAsync(IServiceProvider serviceProvider, Exception ex)
        {
            var handler = serviceProvider.GetRequiredService<IAsyncCommandHandler<TCommand, TKey>>();
            if (handler is IAsyncCommandHandler<TCommand, TKey>.IFailureHandler failureHandler)
            {
                await failureHandler.HandleAsyncOperationFailure(Command, ex);
            }
        }
    }
}
