namespace BeeHive;

internal class HiveSynchronizationContext : SynchronizationContext
{
    private readonly ComputationQueue _computationQueue;

    internal HiveSynchronizationContext(ComputationQueue computationQueue)
    {
        _computationQueue = computationQueue;
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        var continuation = () => d.Invoke(state);
        
        _computationQueue.EnqueueContinuation(continuation);
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        var continuation = () => d.Invoke(state);
        
        _computationQueue.EnqueueContinuation(continuation);
    }
}