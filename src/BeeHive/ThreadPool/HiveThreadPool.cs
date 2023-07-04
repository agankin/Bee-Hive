namespace BeeHive;

internal class HiveThreadPool
{
    private readonly ConcurrentSet<HiveThread> _threads = new();
    private readonly ComputationQueue _computationQueue = new();

    private readonly int _minLiveThreads;
    private readonly int _maxLiveThreads;

    private object _syncObject = new();
    private volatile PoolState _state = PoolState.Stopped;
    private volatile CancellationTokenSource? _poolRunningCancellationTokenSource;

    public HiveThreadPool(ComputationConfiguration configuration)
    {
        _minLiveThreads = configuration.MinLiveThreads;
        _maxLiveThreads = configuration.MaxLiveThreads;
    }

    public HiveThreadPool Start()
    {
        lock (_syncObject)
        {
            if (_state == PoolState.Running)
                return this;

            _state = PoolState.Running;
        }

        _poolRunningCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _poolRunningCancellationTokenSource.Token;

        for (var threadIdx = 0; threadIdx < _minLiveThreads; threadIdx++)
            StartThread(cancellationToken);

        return this;
    }

    public HiveThreadPool Stop()
    {
        lock (_syncObject)
        {
            if (_state == PoolState.Stopped)
                return this;
            
            _state = PoolState.Stopped;
        }

        _poolRunningCancellationTokenSource?.Cancel();

        return this;
    }

    public void Load(Action computation) => _computationQueue.Enqueue(computation);

    private void StartThread(CancellationToken cancellationToken)
    {
        var newThread = new HiveThread(_computationQueue, cancellationToken, OnThreadFinishing);
        newThread.Run();

        _threads.Add(newThread);
    }

    private bool OnThreadFinishing(HiveThread finishedThread)
    {
        var canFinish = CanThreadFinish();

        if (canFinish)
            _threads.Remove(finishedThread);

        return canFinish;
    }

    private bool CanThreadFinish() => _threads.Count > _minLiveThreads;

    private enum PoolState
    {
        Running = 1,

        Stopped
    }
}