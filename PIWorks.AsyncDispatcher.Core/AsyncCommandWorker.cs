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

        public AsyncCommandWorker(ICommandBus<TKey> commandBus, ICommandTracker<TKey> tracker, IServiceScopeFactory serviceScopeFactory)
        {
            _commandBus = commandBus;
            _tracker = tracker;
            _serviceScopeFactory = serviceScopeFactory;
        }

       
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                var envelope = await _commandBus.DequeueAsync(stoppingToken);
                await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Running);
                using var scope = _serviceScopeFactory.CreateScope();

                Type handlerType = typeof(IAsyncCommandHandler<,>).MakeGenericType(envelope.Command.GetType(), typeof(TKey));//?
                dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);



                try
                {
                    await handler.ExecuteAsync((dynamic)envelope.Command, stoppingToken);

                    await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Finished);
                }

                catch (OperationCanceledException)
                {
                    await handler.HandleAsyncOperationCancellation(envelope.Command);
                    await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Error, "The process has been cancelled.");

                }
                catch (Exception ex)
                {
                    await handler.HandleAsyncOperationFailure(envelope.Command, ex);

                    await _tracker.UpdateStatusAsync(envelope.CommandId, CommandStatus.Error, ex.Message);
                    //biz hanlderda hata türeri belirtmiştik ona göre düzenleyeceğim...
                }


            }



        }
    }
}

       
        

          
    
