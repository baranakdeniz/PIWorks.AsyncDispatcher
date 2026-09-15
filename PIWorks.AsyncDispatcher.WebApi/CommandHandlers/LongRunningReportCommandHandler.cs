using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.WebApi.Commands;

namespace PIWorks.AsyncDispatcher.WebApi.CommandHandlers
{
    public class LongRunningReportCommandHandler : IAsyncCommandHandler<LongRunningReportCommand , Guid>
    {
        private readonly ILogger<LongRunningReportCommandHandler> _logger;

        public LongRunningReportCommandHandler(ILogger<LongRunningReportCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(LongRunningReportCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("---> Rapor başlatıldı:{ReportName} ID: {Id}", command.ReportName, command.Key);
            for (int i = 1; i <= 10; i++)
            {
                cancellationToken.ThrowIfCancellationRequested(); // İptal gelirse burada yakalanır
                await Task.Delay(1000, cancellationToken);
                _logger.LogInformation("--> Rapor işleniyor... %{Percent}", i * 10);
            }

            _logger.LogInformation("--> Rapor tamamlandı: {ReportName}", command.ReportName);
        }
    }
}
