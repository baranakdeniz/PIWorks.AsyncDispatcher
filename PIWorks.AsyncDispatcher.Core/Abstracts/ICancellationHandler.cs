using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface ICancellationHandler<TCommand>
    {

        Task HandleAsyncOperationCancellation(TCommand command);

    }
}
