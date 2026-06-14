using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Event;
using PCL_CE.Neo.Core.Lifecycle;

namespace PCL_CE.Neo.Core.Event;

/// <summary>
/// 事件总线服务，用于发布和订阅事件
/// </summary>
public sealed class EventBusService : IService, IAsyncDisposable
{
    private readonly ILogger<EventBusService> _logger;
    private readonly ConcurrentDictionary<string,
        ConcurrentDictionary<Type, ConcurrentDictionary<Guid, (Func<EventDataBase, Task> Handler, WeakReference<object>? OwnerRef)>>> _channels = new();
    private int _isStopping = 0;
    private bool _disposed = false;

    public string Identifier => "eventbus";
    public string Name => "事件总线";
    public bool SupportAsync => true;

    public EventBusService(ILogger<EventBusService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task StartAsync()
    {
        _logger.LogInformation("EventBus 服务启动");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        Interlocked.Exchange(ref _isStopping, 1);
        try
        {
            var channelCount = _channels.Count;
            var handlerCount = _channels.Values.Sum(c => c.Values.Sum(h => h.Count));
            _channels.Clear();
            _logger.LogInformation("EventBus 停止: 清理了 {ChannelCount} 个频道和 {HandlerCount} 个处理器", channelCount, handlerCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EventBus 停止时发生异常");
            throw;
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// 发布事件到指定频道
    /// </summary>
    /// <typeparam name="TEventData">事件数据类型</typeparam>
    /// <param name="channelName">频道名称</param>
    /// <param name="data">事件数据</param>
    /// <exception cref="InvalidOperationException">EventBus 正在停止</exception>
    public async Task PublishAsync<TEventData>(string channelName, TEventData data) where TEventData : EventDataBase
    {
        if (Volatile.Read(ref _isStopping) != 0)
        {
            throw new InvalidOperationException("EventBus 正在停止");
        }

        try
        {
            await CallChannelAsync(channelName, data).ConfigureAwait(false);
            _logger.LogDebug("事件 {EventName} 已发布到频道 {ChannelName}", data.Name, channelName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发布事件到频道 {ChannelName} 时发生异常", channelName);
            throw;
        }
    }

    /// <summary>
    /// 使用 IEventHandler 实例订阅频道
    /// </summary>
    /// <typeparam name="TEventData">事件数据类型</typeparam>
    /// <param name="channel">频道名称</param>
    /// <param name="handler">事件处理器</param>
    /// <param name="disposeOwnerOnUnsubscribe">取消订阅时是否释放处理器</param>
    /// <returns>订阅令牌，用于取消订阅</returns>
    /// <exception cref="InvalidOperationException">EventBus 正在停止</exception>
    /// <exception cref="ArgumentNullException">参数为空</exception>
    public IDisposable Subscribe<TEventData>(string channel, IEventHandler<TEventData> handler, bool disposeOwnerOnUnsubscribe = false)
        where TEventData : EventDataBase
    {
        if (Volatile.Read(ref _isStopping) != 0)
        {
            throw new InvalidOperationException("EventBus 正在停止");
        }
        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentNullException(nameof(channel));
        }
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        try
        {
            if (!_channels.TryGetValue(channel, out var dataHandler))
            {
                _logger.LogDebug("频道 {Channel} 不存在，正在创建", channel);
                AddChannel(channel);
            }

            dataHandler = _channels[channel];
            var dataType = typeof(TEventData);
            var handlers = dataHandler.GetOrAdd(dataType, _ => new ConcurrentDictionary<Guid, (Func<EventDataBase, Task>, WeakReference<object>?)>());

            var ownerRef = new WeakReference<object>(handler);
            var id = Guid.NewGuid();
            handlers.TryAdd(id, (Wrapper, ownerRef));

            _logger.LogDebug("已订阅频道 {Channel}，事件类型 {EventType}，ID {Id}", channel, dataType.Name, id);

            return new Subscription(() =>
            {
                handlers.TryRemove(id, out _);
                if (handlers.IsEmpty)
                {
                    dataHandler.TryRemove(dataType, out _);
                }

                if (disposeOwnerOnUnsubscribe && ownerRef.TryGetTarget(out var target) && target is IDisposable d)
                {
                    var stillReferenced = dataHandler.Values.Any(dict =>
                        dict.Values.Any(e => e.OwnerRef is not null && e.OwnerRef.TryGetTarget(out var other) && ReferenceEquals(other, target)));

                    if (!stillReferenced)
                    {
                        try
                        {
                            d.Dispose();
                            _logger.LogDebug("已释放处理器 {HandlerType}", target.GetType().Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "释放处理器时发生异常");
                        }
                    }
                }
                _logger.LogDebug("已取消订阅频道 {Channel}，ID {Id}", channel, id);
            });

            Task Wrapper(EventDataBase ev)
            {
                if (ownerRef.TryGetTarget(out var target) && target is IEventHandler<TEventData> typed)
                {
                    return typed.HandleEventAsync((TEventData)ev);
                }
                return Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "订阅频道 {Channel} 时发生异常", channel);
            throw;
        }
    }

    /// <summary>
    /// 使用委托订阅频道
    /// </summary>
    /// <typeparam name="TEventData">事件数据类型</typeparam>
    /// <param name="channel">频道名称</param>
    /// <param name="handler">事件处理委托</param>
    /// <returns>订阅令牌，用于取消订阅</returns>
    /// <exception cref="InvalidOperationException">EventBus 正在停止</exception>
    /// <exception cref="ArgumentNullException">参数为空</exception>
    public IDisposable Subscribe<TEventData>(string channel, Func<TEventData, Task> handler)
        where TEventData : EventDataBase
    {
        if (Volatile.Read(ref _isStopping) != 0)
        {
            throw new InvalidOperationException("EventBus 正在停止");
        }
        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentNullException(nameof(channel));
        }
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        try
        {
            if (!_channels.TryGetValue(channel, out var dataHandler))
            {
                _logger.LogDebug("频道 {Channel} 不存在，正在创建", channel);
                AddChannel(channel);
            }

            dataHandler = _channels[channel];
            var dataType = typeof(TEventData);
            var handlers = dataHandler.GetOrAdd(dataType, _ => new ConcurrentDictionary<Guid, (Func<EventDataBase, Task>, WeakReference<object>?)>());

            var id = Guid.NewGuid();
            handlers.TryAdd(id, (Wrapper, null));

            _logger.LogDebug("已订阅频道 {Channel}，事件类型 {EventType}，ID {Id}", channel, dataType.Name, id);

            return new Subscription(() =>
            {
                handlers.TryRemove(id, out _);
                if (handlers.IsEmpty)
                {
                    dataHandler.TryRemove(dataType, out _);
                }
                _logger.LogDebug("已取消订阅频道 {Channel}，ID {Id}", channel, id);
            });

            Task Wrapper(EventDataBase ev) => handler((TEventData)ev);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "订阅频道 {Channel} 时发生异常", channel);
            throw;
        }
    }

    /// <summary>
    /// 创建频道
    /// </summary>
    /// <param name="name">频道名称</param>
    /// <returns>是否成功创建</returns>
    public bool AddChannel(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("频道名称为空，无法创建");
            return false;
        }

        var result = _channels.TryAdd(name, new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, (Func<EventDataBase, Task>, WeakReference<object>?)>>());
        if (result)
        {
            _logger.LogDebug("已创建频道 {Channel}", name);
        }
        else
        {
            _logger.LogDebug("频道 {Channel} 已存在", name);
        }
        return result;
    }

    /// <summary>
    /// 移除频道及其所有处理器
    /// </summary>
    /// <param name="name">频道名称</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveChannel(string name)
    {
        var result = _channels.TryRemove(name, out _);
        if (result)
        {
            _logger.LogDebug("已移除频道 {Channel}", name);
        }
        else
        {
            _logger.LogDebug("频道 {Channel} 不存在", name);
        }
        return result;
    }

    /// <summary>
    /// 检查频道是否存在
    /// </summary>
    /// <param name="name">频道名称</param>
    /// <returns>频道是否存在</returns>
    public bool HasChannel(string name)
    {
        return _channels.ContainsKey(name);
    }

    /// <summary>
    /// 获取所有频道名称
    /// </summary>
    /// <returns>频道名称列表</returns>
    public IReadOnlyList<string> GetChannelNames()
    {
        return _channels.Keys.ToList();
    }

    private async Task CallChannelAsync<TEventData>(string channel, TEventData data)
        where TEventData : EventDataBase
    {
        if (!_channels.TryGetValue(channel, out var eventHandlers))
        {
            _logger.LogError("频道 {Channel} 不存在", channel);
            throw new InvalidOperationException($"频道 {channel} 不存在");
        }

        await CallEventHandlerAsync(data, eventHandlers).ConfigureAwait(false);
    }

    private async Task CallEventHandlerAsync<TEventData>(TEventData data,
        ConcurrentDictionary<Type, ConcurrentDictionary<Guid, (Func<EventDataBase, Task> Handler, WeakReference<object>? OwnerRef)>> dataHandlers)
        where TEventData : EventDataBase
    {
        var eventType = data.GetType();
        var matching = new List<Func<EventDataBase, Task>>();

        foreach (var (registeredType, handlers) in dataHandlers)
        {
            if (registeredType.IsAssignableFrom(eventType))
            {
                foreach (var kv in handlers.ToImmutableArray())
                {
                    var key = kv.Key;
                    var entry = kv.Value;
                    if (entry.OwnerRef is not null)
                    {
                        if (!entry.OwnerRef.TryGetTarget(out var _))
                        {
                            handlers.TryRemove(key, out _);
                            continue;
                        }
                    }
                    matching.Add(entry.Handler);
                }
            }
        }

        if (matching.Count == 0)
        {
            _logger.LogDebug("事件类型 {EventType} 没有找到处理器", eventType.Name);
            return;
        }

        var tasks = matching.Select(async h =>
        {
            try
            {
                await h(data).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "事件处理器执行时发生异常");
            }
        }).ToImmutableArray();

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private sealed class Subscription : IDisposable
    {
        private Action? _dispose;

        public Subscription(Action dispose)
        {
            _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
        }

        public void Dispose()
        {
            var d = Interlocked.Exchange(ref _dispose, null);
            d?.Invoke();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await StopAsync().ConfigureAwait(false);
    }
}