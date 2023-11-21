namespace BeeHive;

internal class HiveThreadPool
{
    private readonly ConcurrentSet<HiveThread> _threads = new();
    private readonly ComputationQueue _computationQueue = new();

    private readonly int _minLiveThreads;
    private readonly int _maxLiveThreads;

    private readonly CancellationTokenSource _poolRunningCancellationTokenSource = new();
    private PoolState _state = PoolState.Created;

    public HiveThreadPool(ComputationConfiguration configuration)
    {
        _minLiveThreads = configuration.MinLiveThreads;
        _maxLiveThreads = configuration.MaxLiveThreads;
    }

    public HiveThreadPool Start()
    {
        if (_state != PoolState.Created)
            throw new InvalidOperationException($"{nameof(HiveThreadPool)} is not in {nameof(PoolState.Created)} state.");

        _state = PoolState.Running;

        var cancellationToken = _poolRunningCancellationTokenSource.Token;
        for (var threadIdx = 0; threadIdx < _minLiveThreads; threadIdx++)
            StartThread(cancellationToken);

        return this;
    }

    public HiveThreadPool Stop()
    {
        if (_state != PoolState.Created)
            throw new InvalidOperationException($"{nameof(HiveThreadPool)} is not in {nameof(PoolState.Running)} state.");

        _poolRunningCancellationTokenSource.Cancel();

        return this;
    }

    public void Queue(Action computation) => _computationQueue.Enqueue(computation);

    private void StartThread(CancellationToken cancellationToken)
    {
        var newThread = new HiveThread(_computationQueue, RequestFinishing, cancellationToken);
        newThread.Run();

        _threads.Add(newThread);
    }

    private bool RequestFinishing(HiveThread thread)
    {
        var canFinish = CanThreadFinish();

        if (canFinish)
            _threads.Remove(thread);

        return canFinish;
    }

    private bool CanThreadFinish() => _threads.Count > _minLiveThreads;

    private enum PoolState
    {
        Created = 1,

        Running = 1,

        Stopped
    }
}