using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.WebApi.Commands;

namespace PIWorks.AsyncDispatcher.WebApi.Controllers
{
    public class TestCommandsController : ControllerBase
    {
        private readonly IAsyncCommandDispatcher<Guid> _dispatcher;
        private readonly ICommandTracker<Guid> _tracker;

        public TestCommandsController(
            IAsyncCommandDispatcher<Guid> dispatcher,
            ICommandTracker<Guid> tracker)
        {
            _dispatcher = dispatcher;
            _tracker = tracker;
        }

        // 1. Asenkron İşi Başlat (Kuyruğa At)
        [HttpPost("start-async-job")]
        public async Task<IActionResult> StartAsyncJob([FromQuery] string reportName, CancellationToken cancellationToken = default)
        {
            var commandId = Guid.NewGuid();
            var command = new LongRunningReportCommand(commandId, reportName);

            await _dispatcher.EnqueueAsync(command, cancellationToken);

            return Ok(new { Message = "The job has enqueued!", CommandId = commandId });
        }
        [HttpPost("cancel-job/{id}")]
        public async Task<IActionResult> CancelJob(Guid id)
        {
            await _dispatcher.CancelAsync(id);
            return Ok(new { Message = "Cancel request has sent!", CommandId = id });
        }
        //senkron komutumuz
        [HttpPost("calculate-sync")]
        public async Task<IActionResult> CalculateSync([FromQuery] decimal amount, CancellationToken cancellationToken)
        {
            var command = new CalculateTaxCommand(amount);

            // Kuyruğa girmeden doğrudan çalışır burada
            var result = await _dispatcher.SendAsync<CalculateTaxCommand, decimal>(command, cancellationToken);

            return Ok(new { OriginalAmount = amount, TotalWithTax = result });
        }
    }
}
