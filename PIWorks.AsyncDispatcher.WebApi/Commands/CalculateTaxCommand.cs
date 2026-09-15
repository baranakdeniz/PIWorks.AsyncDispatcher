using PIWorks.AsyncDispatcher.Core.Abstracts;

namespace PIWorks.AsyncDispatcher.WebApi.Commands
{
    public record CalculateTaxCommand(decimal Amount) : ISyncCommand<decimal>
    {
    }
}
