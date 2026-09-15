using MediatR;
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


            services.AddSingleton(typeof(ICommandBus<>), typeof(InMemoryCommandBus<>));
            services.AddScoped<IAsyncCommandDispatcher<TKey>, DefaultAsyncCommandDispatcher<TKey>>();


            services.AddHostedService<AsyncCommandWorker<TKey>>();
            //handlerları birleştirmek mantıklı mı araştır?

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<MediatRCommandEventPublisher>();
                cfg.TypeEvaluator = type => !type.IsGenericTypeDefinition;
            });

            // Messaging YAPISI
            services.AddTransient<ICommandEventPublisher, MediatRCommandEventPublisher>();

            // Consumer Kayıtları
            services.AddTransient<INotificationHandler<CancelCommandRequestedEvent<TKey>>, CancelCommandRequestedEventConsumer<TKey>>();//bu notificationhandler nasıl kana karıştı burada? 
            services.AddTransient<INotificationHandler<CommandPendingEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandRunningEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandFinishedEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandErrorEvent<TKey>>, CommandStateEventConsumer<TKey>>();
            services.AddTransient<INotificationHandler<CommandCancelledEvent<TKey>>, CommandStateEventConsumer<TKey>>();

            var assemblies = assembliesToScan ?? new[] { Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly() };
            RegisterCommandHandlers(services, assemblies);
            return services;
        }
         private static void RegisterCommandHandlers(IServiceCollection services, Assembly[] assemblies)
        {//projeleri tek tek gez taranacak olanlar!
            foreach (var assembly in assemblies)
            {//seçilen o projedeki bütün dosyaları bir yere topla
                foreach (var type in assembly.GetTypes())
                {
                    //elimizde interf veya abstractsa geç onlar newlenemez çünkü
                    if (type.IsAbstract || type.IsInterface) continue;
                    //clas ın uyguladğı ifaceleri ve mirasları al
                    foreach (var iface in type.GetInterfaces())
                    {//eğer miras generic değilse geç 
                        if (!iface.IsGenericType) continue;
                        // Bu <> nun içindeki tipleri söküp kalıbı alıyoruz.
                        var genericDef = iface.GetGenericTypeDefinition();

                        // bu kalıp aradığımız senkron handler kalıbı mı?
                        if (genericDef == typeof(ISyncCommandHandler<,>))
                        {//evet o zmaan bu senkron kalıbıdır
                            services.AddTransient(iface, type);
                        }
                        // veya bu kalıp aradığımız asnkeron handler kalıbı mı ? 
                        else if (genericDef == typeof(IAsyncCommandHandler<,>))
                        {//eveto zaman bu asenkron kalıbıdr..
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

