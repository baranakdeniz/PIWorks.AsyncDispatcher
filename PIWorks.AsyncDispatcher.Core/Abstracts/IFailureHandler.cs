using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Abstracts
{
    public interface IFailureHandler<in TCommand> 
    {
        Task HandleAsyncOperationFailure(TCommand command, Exception ex);
    }
}
