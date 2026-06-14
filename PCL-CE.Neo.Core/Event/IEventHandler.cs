using System;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Event;

namespace PCL_CE.Neo.Core.Event;

/// <summary>
/// 事件处理器接口
/// </summary>
/// <typeparam name="TEventData">事件数据类型</typeparam>
public interface IEventHandler<in TEventData> : IDisposable
    where TEventData : EventDataBase
{
    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="eventData">事件数据</param>
    Task HandleEventAsync(TEventData eventData);
}