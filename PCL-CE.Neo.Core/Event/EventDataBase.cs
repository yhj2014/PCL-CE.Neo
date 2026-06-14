namespace PCL_CE.Neo.Core.Event;

/// <summary>
/// 事件数据基类，所有事件数据都必须继承此类
/// </summary>
public record EventDataBase(Guid Id, string Name)
{
    /// <summary>
    /// 创建一个新的事件数据实例，自动生成 ID
    /// </summary>
    public EventDataBase(string name) : this(Guid.NewGuid(), name)
    {
    }
}