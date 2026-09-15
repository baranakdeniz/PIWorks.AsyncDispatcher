using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.WebApi.Commands;

namespace PIWorks.AsyncDispatcher.WebApi.CommandHandlers
{
    public class CalculateTaxCommandHandler : ISyncCommandHandler<CalculateTaxCommand, decimal>
    {
        public Task<decimal> HandleAsync(CalculateTaxCommand command, CancellationToken cancellationToken)
        {
            decimal result = command.Amount * 1.20m;
            return Task.FromResult(result);
        }
    }
}
