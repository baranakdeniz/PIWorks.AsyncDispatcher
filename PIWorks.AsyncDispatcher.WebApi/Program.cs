using EventBus.Core.Abstractions;
using EventBus.RabbitMQ.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PIWorks.AsyncDispatcher.Core;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.WebApi.Bus;
using PIWorks.AsyncDispatcher.WebApi.CommandHandlers;
using PIWorks.AsyncDispatcher.WebApi.Commands;
using PIWorks.AsyncDispatcher.WebApi.Infrastructure;
using static PIWorks.AsyncDispatcher.Core.ServiceCollectionExtensions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi"
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
/*builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));*///silinecek


builder.Services.AddRabbitMqEventBus(builder.Configuration);
builder.Services.AddTransient<ICommandEventPublisher, RabbitMqCommandEventPublisher>();


builder.Services.AddAsyncCommandDispatcher<Guid>(
    options =>
    {
        options.AppName = "PIWorks.ReportService";
    },
    assembliesToScan: new[] { typeof(Program).Assembly }
);
builder.Services.AddSingleton<ICommandBus<Guid>, RabbitMqCommandBus<Guid>>();

builder.Services.AddTransient<CommandIntegrationEventHandler<Guid>>();



builder.Services.AddSingleton<IInstanceInfo, DefaultInstanceInfo>();
builder.Services.AddTransient<CommandStatusChangedIntegrationEventHandler>();
builder.Services.AddTransient<CancelCommandRequestedIntegrationEventHandler>();
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PIWorks API v1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<CommandIntegrationEventHandler<Guid>>();
eventBus.Subscribe<CommandStatusChangedIntegrationEventHandler>();
eventBus.Subscribe<CancelCommandRequestedIntegrationEventHandler>();

app.Run();
