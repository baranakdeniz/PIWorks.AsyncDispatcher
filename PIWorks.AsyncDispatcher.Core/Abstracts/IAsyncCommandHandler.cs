using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IAsyncCommandHandler<in TCommand ,TKey>
        where TCommand :IAsyncCommand<TKey> 
    {
        Task ExecuteAsync(TCommand command, CancellationToken cancellationToken);
        public interface IFailureHandler
        {
            Task HandleAsyncOperationFailure(TCommand command, Exception ex);
        }

        public interface ICancellationHandler
        {
            Task HandleAsyncOperationCancellation(TCommand command);
        }
    }
}
