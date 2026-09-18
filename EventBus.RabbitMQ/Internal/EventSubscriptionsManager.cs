using EventBus.Core.Abstractions;
using EventBus.Core.Events;

namespace EventBus.RabbitMQ.Internal;

internal sealed class EventSubscriptionsManager
{
    private readonly Dictionary<string, Type> _eventTypes = new();
    private readonly Dictionary<string, List<Type>> _handlerTypes = new();

    public IReadOnlyCollection<string> EventNames => _eventTypes.Keys;

    public void AddSubscription<THandler>() where THandler : IEventHandler
    {
        var handlerInterfaces = typeof(THandler)
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
            .ToList();

        if (handlerInterfaces.Count == 0)
        {
            throw new InvalidOperationException(
                $"'{typeof(THandler).Name}' does not implement any IEventHandler<> interface and cannot be subscribed.");
        }

        foreach (var handlerInterface in handlerInterfaces)
        {
            Add(handlerInterface.GetGenericArguments()[0], typeof(THandler));
        }
    }

    private void Add(Type eventType, Type handlerType)
    {
        var eventName = eventType.Name;
        _eventTypes[eventName] = eventType;

        if (!_handlerTypes.TryGetValue(eventName, out var handlers))
        {
            handlers = [];
            _handlerTypes[eventName] = handlers;
        }

        if (!handlers.Contains(handlerType))
        {
            handlers.Add(handlerType);
        }
    }

    public bool TryGetEventType(string eventName, out Type? eventType) =>
        _eventTypes.TryGetValue(eventName, out eventType);

    public IReadOnlyList<Type> GetHandlerTypes(string eventName) =>
        _handlerTypes.TryGetValue(eventName, out var handlers) ? handlers : [];
}
