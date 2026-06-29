using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.EventBus;

public interface IEventBus
{
    void Subscribe<T>(Func<T, Task> handler);
    void Subscribe<T>(Action<T> handler);
    void Unsubscribe<T>(Func<T, Task> handler);
    void Unsubscribe<T>(Action<T> handler);
    Task PublishAsync<T>(T @event);
    void Publish<T>(T @event);
    int GetHandlerCount<T>();
}

public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> _handlers = new();

    public void Subscribe<T>(Func<T, Task> handler)
    {
        try
        {
            var type = typeof(T);
            var handlers = _handlers.GetOrAdd(type, _ => new ConcurrentBag<Delegate>());
            handlers.Add(handler);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to subscribe to event: {typeof(T).Name}");
        }
    }

    public void Subscribe<T>(Action<T> handler)
    {
        try
        {
            Func<T, Task> asyncHandler = arg =>
            {
                handler(arg);
                return Task.CompletedTask;
            };
            Subscribe(asyncHandler);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to subscribe to event: {typeof(T).Name}");
        }
    }

    public void Unsubscribe<T>(Func<T, Task> handler)
    {
        try
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                var newHandlers = new ConcurrentBag<Delegate>();
                foreach (var h in handlers)
                {
                    if (!h.Equals(handler))
                        newHandlers.Add(h);
                }
                _handlers[type] = newHandlers;
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to unsubscribe from event: {typeof(T).Name}");
        }
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        try
        {
            Func<T, Task> asyncHandler = arg =>
            {
                handler(arg);
                return Task.CompletedTask;
            };
            Unsubscribe(asyncHandler);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to unsubscribe from event: {typeof(T).Name}");
        }
    }

    public async Task PublishAsync<T>(T @event)
    {
        try
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        if (handler is Func<T, Task> asyncHandler)
                            await asyncHandler(@event);
                    }
                    catch (Exception ex)
                    {
                        LogWrapper.Error(ex, $"Failed to handle event: {type.Name}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to publish event: {typeof(T).Name}");
        }
    }

    public void Publish<T>(T @event)
    {
        try
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        if (handler is Func<T, Task> asyncHandler)
                            asyncHandler(@event).Wait();
                    }
                    catch (Exception ex)
                    {
                        LogWrapper.Error(ex, $"Failed to handle event: {type.Name}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to publish event: {typeof(T).Name}");
        }
    }

    public int GetHandlerCount<T>()
    {
        var type = typeof(T);
        return _handlers.TryGetValue(type, out var handlers) ? handlers.Count : 0;
    }
}