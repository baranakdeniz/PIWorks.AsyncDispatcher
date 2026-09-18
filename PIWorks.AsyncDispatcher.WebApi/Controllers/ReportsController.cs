using EventBus.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PIWorks.AsyncDispatcher.Core;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.WebApi.Bus;
using PIWorks.AsyncDispatcher.WebApi.Commands;
using System.ComponentModel.Design;
using System.Net.NetworkInformation;

namespace PIWorks.AsyncDispatcher.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IAsyncCommandDispatcher<Guid> _dispatcher;
        private readonly ICommandTracker<Guid> _tracker;
        private readonly ICommandCancellationManager<Guid> _cancellationManager;
        private readonly IEventBus _eventBus;

        public ReportsController(IAsyncCommandDispatcher<Guid> dispatcher, ICommandTracker<Guid> tracker, ICommandCancellationManager<Guid> cancellationManager,IEventBus eventBus)
        {
            _dispatcher = dispatcher;
            _tracker = tracker;
            _cancellationManager = cancellationManager;
            _eventBus = eventBus;
        }

        [HttpPost("cancel/{id:guid}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            // 1. ÖNCE LOKALİ KONTROL ET: İş bu makinede mi çalışıyor?
            if (_cancellationManager.Cancel(id))
            {
                return Ok(new { Message = "Komut bu makinede bulundu ve iptal edildi." });
            }

            // 2. BU MAKİNEDE DEĞİLSE: In-Memory Tracker'dan nerede olduğuna bak
            var stateInfo = await _tracker.GetStatusAsync(id);

            // Kayıt hiç yoksa
            if (stateInfo == null)
            {
                return NotFound(new { Message = "Komut bulunamadı." });
            }

            // Nullable ise .Value üzerinden veya doğrudan property ile eriş:
            if (stateInfo.Value.Status == CommandStatus.Finished || stateInfo.Value.Status == CommandStatus.Cancelled)
            {
                return BadRequest(new { Message = "Komut zaten sonlanmış durumda." });
            }

            if (string.IsNullOrEmpty(stateInfo.Value.WorkerId))
            {
                return BadRequest(new { Message = "Komutun hangi makinede olduğu henüz bilinemiyor." });
            }

            // 3. HEDEFİ BULDUK
            await _eventBus.PublishAsync(new CancelCommandRequestedIntegrationEvent
            {
                CommandId = id,
                TargetWorkerId = stateInfo.Value.WorkerId
            });

            return Ok(new { Message = $"İptal emri işin çalıştığı {stateInfo.Value.WorkerId} makinesine gönderildi." });
        }






        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] string reportName, CancellationToken cancellationToken = default)
        {
            var command = new GenerateReportCommand { ReportName = reportName };
            await _dispatcher.EnqueueAsync(command, cancellationToken);
            return Accepted(new { CommandId = command.Key , Status = "Pending" });//burada yazdığım swaggerda response body ile çıkıyor! 
        }
        //[HttpPost("cancel/{id}")]
        //public async Task<IActionResult> Cancel(Guid id , CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //      await _dispatcher.CancelAsync(id, cancellationToken);
                
           
        //       return Accepted(new { Message = $"Command with ID {id} is being cancelled." });
        //    }
        //    catch (KeyNotFoundException)
        //    {
        //        return NotFound(new { Message = $"Command with ID {id} not found." });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        // 409 Conflict veya 400 Bad Request
        //        return Conflict(new { Message = ex.Message });
        //    }


        //}

            

        [HttpGet("running-count")]
        public async Task<IActionResult> GetRunningCount()
        {
            var count = await _tracker.GetRunningCommandsCountAsync();
            return Ok(new { RunningCommands = count });
        }

        [HttpGet("status/{id}")]
        public async Task<IActionResult> GetStatusId(Guid id)
        {

            var status = await _tracker.GetStatusAsync(id);

            //try-catch yapamam çünkü commandstateinfoda "?" kullandım null geçebilir.
            if (!status.HasValue)
            {
                return NotFound(new { Message = $"Command with ID {id} not found." });
            }

            return Ok(new { Command = id, Status = status.Value.Status.ToString(), WorkerNode = status.Value.WorkerId });
        }

        }
    }

