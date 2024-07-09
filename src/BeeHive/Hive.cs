namespace BeeHive;

public class Hive : IDisposable
{
    private readonly ComputationQueue _computationQueue;
    private readonly HiveThreadPool _threadPool;

    internal Hive(HiveConfiguration configuration)
    {
        _computationQueue = new ComputationQueue(configuration.ThreadIdleBeforeStop);
        _threadPool = new HiveThreadPool(configuration, _computationQueue);
    }

    public Hive Run()
    {
        _threadPool.Run();
        return this;
    }

    public HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(Compute<TRequest, TResult> compute) =>
        new HiveQueue<TRequest, TResult>(_computationQueue, compute);

    public void Dispose() => _threadPool.Dispose();
}