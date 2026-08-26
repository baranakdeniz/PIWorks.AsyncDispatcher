using System;
using PIWorks.AsyncDispatcher.Core.Abstracts;

namespace MiniStock.Api
{
    public class TestCommand : IAsyncCommand<Guid>
    {
        public Guid Key { get; } = Guid.NewGuid();
    }
}