using System;
using System.Threading;
using System.Threading.Tasks;
using PIWorks.AsyncDispatcher.Core.Abstracts;

namespace MiniStock.Api
{
    public class TestCommandHandler : IAsyncCommandHandler<TestCommand, Guid>
    {
        public async Task ExecuteAsync(TestCommand command, CancellationToken cancellationToken)
        {
            // Gerçek bir senaryoda burada veritabanı veya loglama işlemleri olur
            for (int i = 1; i <= 7; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(2000, cancellationToken); // 2 saniye bekle
            }
        }
    }
}