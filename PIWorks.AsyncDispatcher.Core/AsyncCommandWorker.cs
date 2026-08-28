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
                await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Running);

                using var scope = _serviceScopeFactory.CreateScope();

                var jobToken = _cancellationManager.RegisterCommand(envelope.CommandId);
                try
                {

                    await envelope.ExecuteAsync(scope.ServiceProvider, jobToken);
                    await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Finished);
                }
                catch (OperationCanceledException)
                {

                    await envelope.HandleCancellationAsync(scope.ServiceProvider);
                    await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Error, "The process has been cancelled.");
                }
                catch (Exception ex)
                {
                    await envelope.HandleFailureAsync(scope.ServiceProvider, ex);
                    await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Error, ex.Message);

                }
                finally
                {
                    _cancellationManager.Remove(envelope.CommandId);

                }

            }
        }
    }
}


       
        

          
    
