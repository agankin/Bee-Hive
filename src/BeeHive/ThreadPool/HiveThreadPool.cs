namespace BeeHive;

internal class HiveThreadPool
{
    private readonly object _threadsSyncObject = new();
    private readonly ConcurrentSet<HiveThread> _threads = new();
    private readonly BlockingQueue<Action> _computationQueue;

    private readonly int _minLiveThreads;
    private readonly int _maxLiveThreads;

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    private PoolState _state = PoolState.Created;
    private volatile int _threadsCount;

    public HiveThreadPool(ComputationConfiguration configuration)
    {
        _minLiveThreads = configuration.MinLiveThreads;
        _maxLiveThreads = configuration.MaxLiveThreads;

        _computationQueue = new(configuration.ThreadWaitForNextMilliseconds);
    }

    public HiveThreadPool Start()
    {
        if (_state != PoolState.Created)
            throw new InvalidOperationException($"{nameof(HiveThreadPool)} is not in {nameof(PoolState.Created)} state.");

        _state = PoolState.Running;

        for (var threadIdx = 0; threadIdx < _minLiveThreads; threadIdx++)
        {
            StartThread(_cancellationTokenSource.Token);
            _threadsCount++;
        }

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

    public void Queue(Action computation)
    {
        if (RequestStartingThread())
            StartThread(_cancellationTokenSource.Token);

        _computationQueue.Enqueue(computation);
    }

    private void StartThread(CancellationToken cancellationToken)
    {
        var newThread = new HiveThread(_computationQueue, RequestFinishingThread, cancellationToken);
        newThread.Run();

        _threads.Add(newThread);
    }

    private bool RequestFinishingThread(HiveThread thread)
    {
        var canFinish = RequestFinishingThread();
        if (canFinish)
            _threads.Remove(thread);

        return canFinish;
    }

    private bool RequestStartingThread()
    {
        lock (_threadsSyncObject)
        {
            if (_threadsCount >= _maxLiveThreads)
                return false;

            var shouldStart = _computationQueue.Count > 0;
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

    private enum PoolState
    {
        Created = 1,

        Running = 1,

        Stopped
    }
}