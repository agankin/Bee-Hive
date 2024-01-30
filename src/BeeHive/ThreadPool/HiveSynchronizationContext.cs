namespace BeeHive;

internal class HiveSynchronizationContext : SynchronizationContext
{
    private readonly HiveComputationQueue _computationQueue;

    internal HiveSynchronizationContext(HiveComputationQueue computationQueue)
    {
        _computationQueue = computationQueue;
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        var continuation = () => d.Invoke(state);
        var continuationComputation = new HiveComputation(continuation);
        
        _computationQueue.Enqueue(continuationComputation);
    }

    public override void Send(SendOrPostCallback d, object? state) => throw new NotSupportedException();
}