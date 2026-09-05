using PIWorks.AsyncDispatcher.Core.Abstracts;

namespace PIWorks.AsyncDispatcher.WebApi.Commands
{
    public class GenerateReportCommand : IAsyncCommand<Guid>
    {
        public Guid Key { get;} =Guid.NewGuid();

        public string? ReportName { get; set; }

    }
}
