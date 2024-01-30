namespace BeeHive;

internal class HiveComputationQueue : BlockingQueue<HiveComputation>
{
    public HiveComputationQueue(int waitForNextMilliseconds) : base(waitForNextMilliseconds)
    {
    }

    public event Action? Enqueueing;

    protected override void OnEnqueueing(HiveComputation computation)
    {
        Enqueueing?.Invoke();
        base.OnEnqueueing(computation);
    }
}