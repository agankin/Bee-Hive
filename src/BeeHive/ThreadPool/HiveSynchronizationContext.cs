namespace BeeHive;

internal class HiveSynchronizationContext : SynchronizationContext
{
    private readonly HiveThreadPool _threadPool;

    internal HiveSynchronizationContext(HiveThreadPool threadPool)
    {
        _threadPool = threadPool;
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        var action = () => d.Invoke(state);
        _threadPool.Queue(action);
    }

    public override void Send(SendOrPostCallback d, object? state) => throw new NotSupportedException();
}