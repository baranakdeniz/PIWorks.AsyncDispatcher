using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using PIWorks.AsyncDispatcher.Core.Options;

using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncCommandDispatcher<TKey>(this IServiceCollection services, Action<AsyncDispatcherOptions>? configureOptions = null)
        {
            var options = new AsyncDispatcherOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
                services.Configure(configureOptions);
            }
            else
            {
                services.AddOptions<AsyncDispatcherOptions>();
            }

            services.AddSingleton<ICommandCancellationManager<TKey>, CommandCancellationManager<TKey>>();
            services.AddSingleton<ICommandTracker<TKey>, InMemoryCommandTracker<TKey>>();

           
            services.AddSingleton(typeof(ICommandBus<>), typeof(InMemoryCommandBus<>));
            services.AddScoped<IAsyncCommandDispatcher<TKey>, DefaultAsyncCommandDispatcher<TKey>>();

           
            services.AddHostedService<AsyncCommandWorker<TKey>>();

          
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<MediatRCommandEventPublisher>();
                cfg.TypeEvaluator = type => !type.IsGenericTypeDefinition;
            });

            // Messaging YAPISI
            services.AddTransient<ICommandEventPublisher, MediatRCommandEventPublisher>();

            // Consumer Kayıtları
            services.AddTransient<INotificationHandler<CancelCommandRequestedEvent<TKey>>, CancelCommandRequestedEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandPendingEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandRunningEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandFinishedEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandErrorEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandCancelledEvent<TKey>>, CommandStateEventConsumer<TKey>>();

            return services;
        
    }
    }
}
//builder.Services.AddAsyncCommandDispatcher(); bu kütüphaneyi kullanan developer bunu program.cs ine yazarak tüm sistemi ayağa kaldrır.
//yoksa hangi service in lifetime ı nedir bilmek zorundayız programcs e yazarken
//burada IoC kurulum metodunu yazdık .net in ıoc containerına otomatik bir şekilde kaydedilmesini sağlar.

