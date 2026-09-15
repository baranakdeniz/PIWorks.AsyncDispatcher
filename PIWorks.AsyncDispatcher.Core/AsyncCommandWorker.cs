
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using PIWorks.AsyncDispatcher.Core.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class AsyncCommandWorker<TKey> : BackgroundService
    {
        private readonly ICommandBus<TKey> _commandBus;
        private readonly ICommandTracker<TKey> _tracker;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICommandCancellationManager<TKey> _cancellationManager;
        private readonly ICommandEventPublisher _eventPublisher;
        private readonly ILogger<AsyncCommandWorker<TKey>> _logger;
        private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid().ToString().Substring(0, 6)}";

        public AsyncCommandWorker(ICommandBus<TKey> commandBus,
            ICommandTracker<TKey> tracker,
            ICommandCancellationManager<TKey> cancellationManager, 
            IServiceScopeFactory serviceScopeFactory , 
            ICommandEventPublisher eventpublisher,
            ILogger<AsyncCommandWorker<TKey>> logger
            )
        {
            _commandBus = commandBus;
            _tracker = tracker;
            _serviceScopeFactory = serviceScopeFactory;
            _cancellationManager = cancellationManager;
            _eventPublisher = eventpublisher;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
               
                var envelope = await _commandBus.DequeueAsync(stoppingToken);    
                
                //Fire and Forget Mechanism (Multithreadng)!!!!
                _ = Task.Run(async () =>
                {                    

                    using var scope = _serviceScopeFactory.CreateScope();
                    
                    var jobToken = _cancellationManager.GetToken(envelope.CommandId);
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(jobToken, stoppingToken);
                    try
                    {
                        await _eventPublisher.PublishAsync(new CommandRunningEvent<TKey>(envelope.CommandId, _workerId));
                        await envelope.ExecuteAsync(scope.ServiceProvider, linkedCts.Token);
                        await _eventPublisher.PublishAsync(new CommandFinishedEvent<TKey>(envelope.CommandId, _workerId));
                    }
                    catch (OperationCanceledException)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            // 1. Sunucu kapanıyor! Bu işi kullanıcı iptal etmedi!!!
                            // State'i 'Cancelled' yapmayıp log atıyoruz ve öyle bırakıyoruz (sunucu açılınca tekrar denenir veya Pending kalır bu durumda)
                            _logger.LogWarning("Command {CommandId} host kapatıldığı için yarıda kesildi.", envelope.CommandId);
                        }
                        else
                        {
                            // 2. Gerçek kullanıcı iptali (jobToken tetiklendi)!!
                            await envelope.HandleCancellationAsync(scope.ServiceProvider);
                            await _eventPublisher.PublishAsync(new CommandCancelledEvent<TKey>(envelope.CommandId, _workerId));
                        }
                    }
                    catch (Exception ex)
                    {
                        await envelope.HandleFailureAsync(scope.ServiceProvider, ex);
                        await _eventPublisher.PublishAsync(new CommandErrorEvent<TKey>(envelope.CommandId, _workerId, ex.Message));
                    }
                    finally
                    {
                        _cancellationManager.Remove(envelope.CommandId);
     
                    }
                }, stoppingToken);
               

           
            }
        }

        //public override void dispose()
        //{

        //    base.dispose();
        //}
    }
}


       
        

          
    
