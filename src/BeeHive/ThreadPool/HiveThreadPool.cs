namespace BeeHive;

internal class HiveThreadPool
{
    private readonly object _threadsSyncObject = new();
    private readonly ConcurrentSet<HiveThread> _threads = new();

    private readonly int _minLiveThreads;
    private readonly int _maxLiveThreads;
    
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    private PoolState _state = PoolState.Created;
    private volatile int _threadsCount;

    public HiveThreadPool(HiveConfiguration configuration, HiveComputationQueue computationQueue)
    {
        _minLiveThreads = configuration.MinLiveThreads;
        _maxLiveThreads = configuration.MaxLiveThreads;

        ComputationQueue = computationQueue;
        ComputationQueue.Enqueueing += OnComputationEnqueueing;
    }

    internal HiveComputationQueue ComputationQueue { get; }

    public HiveThreadPool Start()
    {
        if (_state != PoolState.Created)
            throw new InvalidOperationException($"{nameof(HiveThreadPool)} is not in {nameof(PoolState.Created)} state.");

        _state = PoolState.Running;

        for (_threadsCount = 0; _threadsCount < _minLiveThreads; _threadsCount++)
            StartThread(_cancellationTokenSource.Token);

        return this;
    }

    public void Stop()
    {
        if (_state == PoolState.Stopped)
            return;

        if (_state != PoolState.Created)
            throw new InvalidOperationException($"{nameof(HiveThreadPool)} is not in {nameof(PoolState.Running)} state.");

        _cancellationTokenSource.Cancel();
    }

    internal bool RequestFinishingThread(HiveThread thread)
    {
        var canFinish = RequestFinishingThread();
        if (canFinish)
            _threads.Remove(thread);

        return canFinish;
    }

    private void OnComputationEnqueueing()
    {
        if (RequestStartingThread())
            StartThread(_cancellationTokenSource.Token);
    }

    private bool RequestStartingThread()
    {
        lock (_threadsSyncObject)
        {
            if (_threadsCount >= _maxLiveThreads)
                return false;

            var shouldStart = ComputationQueue.Count > 0;
            if (shouldStart)
                _threadsCount++;

            return shouldStart;
        }
    }

    private bool RequestFinishingThread()
    {
        lock (_threadsSyncObject)
        {
            var canFinish = _threadsCount > _minLiveThreads;
            if (canFinish)
                _threadsCount--;
            
            return canFinish;
        }
    }

    private void StartThread(CancellationToken cancellationToken)
    {
        var newThread = new HiveThread(this, cancellationToken);
        newThread.Run();

        _threads.Add(newThread);
    }

    private enum PoolState
    {
        Created = 1,

        Running = 1,

        Stopped
    }
}