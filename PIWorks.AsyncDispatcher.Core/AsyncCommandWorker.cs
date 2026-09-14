
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(5,5);
        private readonly string _currentAppName;
        private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid().ToString().Substring(0, 6)}";

        public AsyncCommandWorker(ICommandBus<TKey> commandBus,
            ICommandTracker<TKey> tracker,
            ICommandCancellationManager<TKey> cancellationManager, 
            IServiceScopeFactory serviceScopeFactory , 
            ICommandEventPublisher eventpublisher,
            IOptions<AsyncDispatcherOptions> options
            )
        {
            _commandBus = commandBus;
            _tracker = tracker;
            _serviceScopeFactory = serviceScopeFactory;
            _cancellationManager = cancellationManager;
            _eventPublisher = eventpublisher;
            _currentAppName = options.Value.AppName;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
               
                var envelope = await _commandBus.DequeueAsync(stoppingToken);
               await semaphoreSlim.WaitAsync(stoppingToken);
                
                //Fire and Forget Mechanism (Multithreadng)!!!!
                _ = Task.Run(async () =>
                {                    

                    using var scope = _serviceScopeFactory.CreateScope();
                    
                    var jobToken = _cancellationManager.RegisterCommand(envelope.CommandId);
                    try
                    {
                        await _eventPublisher.PublishAsync(new CommandRunningEvent<TKey>(envelope.CommandId, _currentAppName, _workerId));
                        await envelope.ExecuteAsync(scope.ServiceProvider, jobToken);
                        await _eventPublisher.PublishAsync(new CommandFinishedEvent<TKey>(envelope.CommandId, _currentAppName, _workerId));
                    }
                    catch (OperationCanceledException)
                    {

                        await envelope.HandleCancellationAsync(scope.ServiceProvider);
                        await _eventPublisher.PublishAsync(new CommandCancelledEvent<TKey>(envelope.CommandId, _currentAppName, _workerId));
                    }
                    catch (Exception ex)
                    {
                        await envelope.HandleFailureAsync(scope.ServiceProvider, ex);
                        await _eventPublisher.PublishAsync(new CommandErrorEvent<TKey>(envelope.CommandId, _currentAppName, _workerId, ex.Message));
                    }
                    finally
                    {
                        _cancellationManager.Remove(envelope.CommandId);
                        semaphoreSlim.Release();

                    }
                }, stoppingToken);
               

           
            }
        }

        public override void Dispose()
        {
            semaphoreSlim.Dispose();
            base.Dispose();
        }
    }
}


       
        

          
    
