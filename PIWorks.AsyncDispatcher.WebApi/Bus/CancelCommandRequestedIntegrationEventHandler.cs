using EventBus.Core.Abstractions;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.WebApi.Infrastructure;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public class CancelCommandRequestedIntegrationEventHandler : IEventHandler<CancelCommandRequestedIntegrationEvent>
    {
        private readonly ICommandCancellationManager<Guid> _cancellationManager;
        private readonly IInstanceInfo _instanceInfo;
        private readonly ILogger<CancelCommandRequestedIntegrationEventHandler> _logger;

        public CancelCommandRequestedIntegrationEventHandler(
            ICommandCancellationManager<Guid> cancellationManager,
            IInstanceInfo instanceInfo,
            ILogger<CancelCommandRequestedIntegrationEventHandler> logger)
        {
            _cancellationManager = cancellationManager;
            _instanceInfo = instanceInfo;
            _logger = logger;
        }

        public Task HandleAsync(CancelCommandRequestedIntegrationEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[CANCEL EVENT CAME UP] ID to be cancelled: {@event.CommandId} | Target: {@event.TargetWorkerId} | My Id: {_instanceInfo.WorkerId}");
            
            if (@event.TargetWorkerId != _instanceInfo.WorkerId)
                return Task.CompletedTask;

            
           var isCancelled= _cancellationManager.Cancel(@event.CommandId);
            _logger.LogInformation($"[CANCEL RESULT] Manager Is Successfull?: {isCancelled}");
            return Task.CompletedTask;
        }
    }
}
