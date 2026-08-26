using Microsoft.Extensions.DependencyInjection;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public class AsyncCommandManager //BU SINIF SİLİNECEKTİR ŞU AN KALSIN!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        //    private readonly ConcurrentDictionary<object, IDisposable> _runningOperations = new();

        //    private readonly IServiceScopeFactory _scopeFactory;

        //    public AsyncCommandManager(IServiceScopeFactory scopeFactory)
        //    {
        //        _scopeFactory = scopeFactory;
        //    }

        //    public void DispatchAsync<TCommand, TKey>(TCommand command, CancellationToken cancellationToken = default)
        //where TCommand : IAsyncCommand<TKey>
        //    {
        //        var operation = new CommandTrackingInfo<TCommand>(command, cancellationToken);
        //        _runningOperations.TryAdd(command.Key, operation);


        //        Task.Run(async () =>
        //        {

        //            using var scope = _scopeFactory.CreateScope();


        //            var handler = scope.ServiceProvider.GetRequiredService<IAsyncCommandHandler<TCommand, TKey>>();

        //            try
        //            {

        //                await handler.ExecuteAsync(command, operation.Cts.Token);
        //            }
        //            catch (OperationCanceledException)
        //            {
        //                if (handler is IAsyncCommandHandler<TCommand, TKey>.ICancellationHandler cancelHandler)
        //                {
        //                    await cancelHandler.HandleAsyncOperationCancellation(command);
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //                if (handler is IAsyncCommandHandler<TCommand, TKey>.IFailureHandler failureHandler)
        //                {
        //                    await failureHandler.HandleAsyncOperationFailure(command, ex);
        //                }
        //            }
        //            finally
        //            {

        //                _runningOperations.TryRemove(command.Key, out _);
        //                operation.Dispose();
        //            }
        //        });
        //    }
        //    public bool CancelRunningOperation<TKey>(TKey key)
        //    {

        //        if (_runningOperations.TryGetValue(key, out var operation))
        //        {
        //            if (operation is CommandTrackingInfo<IAsyncCommand<TKey>> runningOp)
        //            {
        //                runningOp.Cts.Cancel();
        //                return true;
        //            }
        //        }
        //        return false;
        //    }
        //}
    }
}
