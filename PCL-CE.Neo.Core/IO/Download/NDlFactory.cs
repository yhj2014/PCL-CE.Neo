namespace PCL_CE.Neo.Core.IO.Download;

public abstract class NDlFactory
{
    public abstract IDlConnection? CreateConnection(string resId);
    public abstract IDlWriter? CreateWriter(string resId);
}

public abstract class NDlFactory<TSourceArgument, TTargetArgument> : NDlFactory
{
    protected abstract IDlResourceMapping<TSourceArgument> SourceMapping { get; }
    protected abstract IDlResourceMapping<TTargetArgument> TargetMapping { get; }

    protected abstract IDlConnection CreateConnection(TSourceArgument source);

    public override IDlConnection? CreateConnection(string resId)
    {
        var source = SourceMapping.Parse(resId);
        return source == null ? null : CreateConnection(source);
    }

    protected abstract IDlWriter CreateWriter(TTargetArgument target);

    public override IDlWriter? CreateWriter(string resId)
    {
        var target = TargetMapping.Parse(resId);
        return target == null ? null : CreateWriter(target);
    }
}