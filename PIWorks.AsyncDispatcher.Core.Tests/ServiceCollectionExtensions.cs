using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncCommandDispatcher<TKey>(this IServiceCollection services)
        {
       
            services.AddSingleton<ICommandCancellationManager<TKey>, CommandCancellationManager<TKey>>();

            //services.AddTransient<INotificationHandler<CommandCancelledEvent<TKey>>, CancelCommandRequestedEventConsumer<TKey>>();
                                                                                                                               
        

            //services.AddTransient<ICommandEventPublisher<TKey>, MediatRCommandEventPublisher<TKey>>();

          
            services.AddSingleton<ICommandTracker<TKey>, InMemoryCommandTracker<TKey>>();

            services.AddScoped<IAsyncCommandDispatcher<TKey>, DefaultAsyncCommandDispatcher<TKey>>();

         
            services.AddSingleton(typeof(ICommandBus<>), typeof(InMemoryCommandBus<>));
           
            services.AddHostedService<AsyncCommandWorker<TKey>>();

            return services;
        }
    }
}
//builder.Services.AddAsyncCommandDispatcher(); bu kütüphaneyi kullanan developer bunu program.cs ine yazarak tüm sistemi ayağa kaldrır.
//yoksa hangi service in lifetime ı nedir bilmek zorundayız programcs e yazarken
//burada IoC kurulum metodunu yazdık .net in ıoc containerına otomatik bir şekilde kaydedilmesini sağlar.

