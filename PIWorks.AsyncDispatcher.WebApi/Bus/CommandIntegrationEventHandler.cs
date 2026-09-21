using EventBus.Core.Abstractions;
using EventBus.Core.Events;
using PIWorks.AsyncDispatcher.Core;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using PIWorks.AsyncDispatcher.WebApi.Infrastructure;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PIWorks.AsyncDispatcher.WebApi.Bus
{
    public class CommandIntegrationEventHandler<TKey> : IEventHandler<CommandIntegrationEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandCancellationManager<TKey> _cancellationManager;
        private readonly IEventBus _eventBus;
        private readonly IInstanceInfo _instanceInfo;

        // Benim Yazdığım Mini-Mediatr Yapısı
        private readonly IInternalEventPublisher _internalPublisher;

        
        private static readonly ConcurrentDictionary<Type, Type> EnvelopeTypeCache = new();

        public CommandIntegrationEventHandler(
            IServiceProvider serviceProvider,
            ICommandCancellationManager<TKey> cancellationManager,
            IEventBus eventBus,
            IInstanceInfo instanceInfo,
            IInternalEventPublisher internalPublisher)
        {
            _serviceProvider = serviceProvider;
            _cancellationManager = cancellationManager;
            _eventBus = eventBus;
            _instanceInfo = instanceInfo;
            _internalPublisher = internalPublisher;
        }

        public async Task HandleAsync(CommandIntegrationEvent @event, CancellationToken cancellationToken)
        {
            // Tipi bul
            var commandType = Type.GetType(@event.CommandTypeName)!;
            var command = JsonSerializer.Deserialize(@event.CommandPayloadJson, commandType);

            // PERFORMANS: Tipi her seferinde üretmek yerine Cache'den alıyoruz!
            var envelopeType = EnvelopeTypeCache.GetOrAdd(
                commandType,
                t => typeof(CommandEnvelope<,>).MakeGenericType(t, typeof(TKey))
            );

            var envelope = (CommandEnvelope<TKey>)Activator.CreateInstance(envelopeType, command)!;
            var token = _cancellationManager.GetToken(envelope.CommandId);


            using var scope = _serviceProvider.CreateScope();
            var workerId = _instanceInfo.WorkerId;
            Console.WriteLine($"[HANDLER] Çalışan Komut Id: {envelope.CommandId} | WorkerId: {workerId}");
            try
            {
             
                await _internalPublisher.PublishAsync(new CommandRunningEvent<TKey>(envelope.CommandId, workerId), token);

                
                await BroadcastStatusAsync(envelope.CommandId, "Running", workerId, token);

                // İŞİ ÇALIŞTIRMA BÖlGESİ!
                await envelope.ExecuteAsync(scope.ServiceProvider, token);

               
                await _internalPublisher.PublishAsync(new CommandFinishedEvent<TKey>(envelope.CommandId, workerId), token);
                await BroadcastStatusAsync(envelope.CommandId, "Finished", workerId, token);
            }
            catch (OperationCanceledException)
            {
                await envelope.HandleCancellationAsync(scope.ServiceProvider);

             
                await _internalPublisher.PublishAsync(new CommandCancelledEvent<TKey>(envelope.CommandId, workerId), CancellationToken.None);
                await BroadcastStatusAsync(envelope.CommandId, "Cancelled", workerId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                await envelope.HandleFailureAsync(scope.ServiceProvider, ex);

                await _internalPublisher.PublishAsync(new CommandErrorEvent<TKey>(envelope.CommandId, workerId, ex.Message), CancellationToken.None);
                await BroadcastStatusAsync(envelope.CommandId, "Failed", workerId, CancellationToken.None, ex.Message);
            }
            finally
            {
                _cancellationManager.Remove(envelope.CommandId);
            }
        }

        // RabbitMQ'ya basan yardımcı metot
        private async Task BroadcastStatusAsync(TKey commandId, string status, string workerId, CancellationToken cancellationToken, string? error = null)
        {
            if (commandId is Guid id)
            {
                await _eventBus.PublishAsync(new CommandStatusChangedIntegrationEvent
                {
                    CommandId = id,
                    Status = status,
                    WorkerId = workerId,
                    ErrorMessage = error
                }, cancellationToken);
            }
        }
    }
}

