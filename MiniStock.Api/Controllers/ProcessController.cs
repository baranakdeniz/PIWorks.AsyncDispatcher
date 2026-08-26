using System;
using Microsoft.AspNetCore.Mvc;
using PIWorks.AsyncDispatcher.Core;

namespace MiniStock.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessController : ControllerBase
    {
        private readonly AsyncCommandManager _manager;

        public ProcessController(AsyncCommandManager manager)
        {
            _manager = manager;
        }

        [HttpPost("start")]
        public IActionResult StartProcess()
        {
            var command = new TestCommand();

            // İşlemi arka plana fırlatıyoruz
            _manager.DispatchAsync<TestCommand, Guid>(command);

            // HTTP 202 (Accepted) dönerek isteğin sıraya alındığını bildiriyoruz
            return Accepted(new
            {
                Message = "İşlem arka planda başlatıldı.",
                OperationId = command.Key
            });
        }
    }
}