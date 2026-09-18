
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using PIWorks.AsyncDispatcher.Core.Options;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncCommandDispatcher<TKey>(this IServiceCollection services, Action<AsyncDispatcherOptions>? configureOptions = null, Assembly[]? assembliesToScan = null)
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
           

            //services.AddSingleton(typeof(ICommandBus<>), typeof(InMemoryCommandBus<>));
            services.AddScoped<IAsyncCommandDispatcher<TKey>, DefaultAsyncCommandDispatcher<TKey>>();
            services.AddScoped<IInternalEventPublisher, InternalEventPublisher>();

            //services.AddHostedService<AsyncCommandWorker<TKey>>();
            //handlerları birleştirmek mantıklı mı araştır?

            // Consumer Kayıtları
            services.AddTransient<IDispatcherEventHandler<CancelCommandRequestedEvent<TKey>>, CancelCommandRequestedEventConsumer<TKey>>();
            services.AddTransient<IDispatcherEventHandler<CommandPendingEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<IDispatcherEventHandler<CommandRunningEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<IDispatcherEventHandler<CommandFinishedEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<IDispatcherEventHandler<CommandErrorEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<IDispatcherEventHandler<CommandCancelledEvent<TKey>>, CommandStateEventConsumer<TKey>>();

            var assemblies = assembliesToScan ?? new[] { Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly() };
            RegisterCommandHandlers(services, assemblies);
            return services;
        }
        private static void RegisterCommandHandlers(IServiceCollection services, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray()!;
                }

                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface) continue;

                    foreach (var iface in type.GetInterfaces())
                    {
                        if (!iface.IsGenericType) continue;

                        var genericDef = iface.GetGenericTypeDefinition();

                        if (genericDef == typeof(ISyncCommandHandler<,>) || genericDef == typeof(IAsyncCommandHandler<,>))
                        {
                            services.AddTransient(iface, type);
                        }
                    }
                }
            }
        }
    }
    }
       




//builder.Services.AddAsyncCommandDispatcher(); bu kütüphaneyi kullanan developer bunu program.cs ine yazarak tüm sistemi ayağa kaldrır.
//yoksa hangi service in lifetime ı nedir bilmek zorundayız programcs e yazarken
//burada IoC kurulum metodunu yazdık .net in ıoc containerına otomatik bir şekilde kaydedilmesini sağlar.

