using Microsoft.Extensions.DependencyInjection;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public abstract class CommandEnvelope<TKey>
    {
        public TKey CommandId { get; init; }

        
        public abstract Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
        public abstract Task HandleCancellationAsync(IServiceProvider serviceProvider);
        public abstract Task HandleFailureAsync(IServiceProvider serviceProvider, Exception ex);
    } 
    public class CommandEnvelope<TCommand,TKey> : CommandEnvelope<TKey> where TCommand : IAsyncCommand<TKey>
    {
        public TCommand Command { get; init; }

        public override async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            
            var handler = serviceProvider.GetRequiredService<IAsyncCommandHandler<TCommand, TKey>>();
            await handler.ExecuteAsync(Command, cancellationToken);

        }

        public override async Task HandleCancellationAsync(IServiceProvider serviceProvider)
        {
            var handler = serviceProvider.GetRequiredService<IAsyncCommandHandler<TCommand, TKey>>();
            if (handler is ICancellationHandler<TCommand> cancelHandler)
            {
                await cancelHandler.HandleAsyncOperationCancellation(Command);
            }
        }

        public override async Task HandleFailureAsync(IServiceProvider serviceProvider, Exception ex)
        {
            var handler = serviceProvider.GetRequiredService<IAsyncCommandHandler<TCommand, TKey>>();
            if (handler is IFailureHandler<TCommand> failureHandler)
            {
                await failureHandler.HandleAsyncOperationFailure(Command, ex);
            }
        }
    }
}
