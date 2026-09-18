using EventBus.Core.Abstractions;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.WebApi.Infrastructure;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public class CancelCommandRequestedIntegrationEventHandler : IEventHandler<CancelCommandRequestedIntegrationEvent>
    {
        private readonly ICommandCancellationManager<Guid> _cancellationManager;
        private readonly IInstanceInfo _instanceInfo;

        public CancelCommandRequestedIntegrationEventHandler(
            ICommandCancellationManager<Guid> cancellationManager,
            IInstanceInfo instanceInfo)
        {
            _cancellationManager = cancellationManager;
            _instanceInfo = instanceInfo;
        }

        public Task HandleAsync(CancelCommandRequestedIntegrationEvent @event, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[CANCEL EVENT GELDİ] İptal İstenen Id: {@event.CommandId} | Hedef: {@event.TargetWorkerId} | Benim Id: {_instanceInfo.WorkerId}");
            // Bana gelmediyse umursamam
            if (@event.TargetWorkerId != _instanceInfo.WorkerId)
                return Task.CompletedTask;

            // eğerki emir banaysa yerel iptali tetikleme işi!!!!!!!!!
           var isCancelled= _cancellationManager.Cancel(@event.CommandId);
            Console.WriteLine($"[CANCEL SONUCU] Manager İptal Başarılı mı?: {isCancelled}");
            return Task.CompletedTask;
        }
    }
}
