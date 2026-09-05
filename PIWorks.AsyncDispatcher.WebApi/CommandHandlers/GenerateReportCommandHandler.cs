using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.WebApi.Commands;

namespace PIWorks.AsyncDispatcher.WebApi.CommandHandlers
{
    public class GenerateReportCommandHandler : IAsyncCommandHandler<GenerateReportCommand, Guid>, IAsyncCommandHandler<GenerateReportCommand, Guid>.ICancellationHandler

    {
        private readonly ILogger<GenerateReportCommandHandler> _logger;

        public GenerateReportCommandHandler(ILogger<GenerateReportCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(GenerateReportCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing GenerateReportCommand with Key: {Key} and ReportName: {ReportName}", command.Key, command.ReportName);
         
                await Task.Delay(15000, cancellationToken);
        
            _logger.LogInformation("Finished executing GenerateReportCommand with Key: {Key}", command.Key);
        }

        public Task HandleAsyncOperationCancellation(GenerateReportCommand command)
        {
            _logger.LogWarning("Operation for GenerateReportCommand with Key: {Key} has been cancelled.", command.Key);
            return Task.CompletedTask;
        }
    }
}
//EN BAŞTA HANDLER MI BAŞLIOR ? 