using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Tests
{
    
    public class TestableAsyncCommandWorker : AsyncCommandWorker<Guid>
    {
        public TestableAsyncCommandWorker(
            ICommandBus<Guid> commandBus,
            ICommandTracker<Guid> tracker,
            ICommandCancellationManager<Guid> cancellationManager,
            IServiceScopeFactory serviceScopeFactory)
            : base(commandBus, tracker, cancellationManager, serviceScopeFactory)
        {
        }

        public Task ExecuteForTest(CancellationToken cancellationToken)
        {
            return ExecuteAsync(cancellationToken);
        }
    }
}
