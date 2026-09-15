using PIWorks.AsyncDispatcher.Core.Abstracts;

namespace PIWorks.AsyncDispatcher.WebApi.Commands
{
    public record LongRunningReportCommand(Guid Key, string ReportName) : IAsyncCommand<Guid>
    {

    }
}
