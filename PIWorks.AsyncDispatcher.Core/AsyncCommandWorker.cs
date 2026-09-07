using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PIWorks.AsyncDispatcher.Core.Abstracts;
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
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(5,5);
        private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid().ToString().Substring(0, 6)}";

        public AsyncCommandWorker(ICommandBus<TKey> commandBus, ICommandTracker<TKey> tracker, IServiceScopeFactory serviceScopeFactory, ICommandCancellationManager<TKey> cancellationManager)
        {
            _commandBus = commandBus;
            _tracker = tracker;
            _serviceScopeFactory = serviceScopeFactory;
            _cancellationManager = cancellationManager;
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
                        await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Running, _workerId);
                        await envelope.ExecuteAsync(scope.ServiceProvider, jobToken);
                        await _tracker.UpdateStatusAsync(envelope.CommandId,CommandStatus.Finished, _workerId);
                    }
                    catch (OperationCanceledException)
                    {

                        await envelope.HandleCancellationAsync(scope.ServiceProvider);
                        await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Cancelled, _workerId);
                    }
                    catch (Exception ex)
                    {
                        await envelope.HandleFailureAsync(scope.ServiceProvider, ex);
                        await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Error, _workerId, ex.Message);
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


       
        

          
    
