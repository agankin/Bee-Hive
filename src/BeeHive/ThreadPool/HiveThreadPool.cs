namespace BeeHive;

internal class HiveThreadPool : IDisposable
{
    private readonly object _threadsSyncObject = new();
    private readonly ConcurrentSet<HiveThread> _threads = new();

    private readonly int _minLiveThreads;
    private readonly int _maxLiveThreads;
    
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    private volatile int _state = (int)PoolState.Created;
    private volatile int _threadsCount;

    public HiveThreadPool(HiveConfiguration configuration, ComputationQueue computationQueue)
    {
        _minLiveThreads = configuration.MinLiveThreads;
        _maxLiveThreads = Math.Max(_minLiveThreads, configuration.MaxLiveThreads);

        ComputationQueue = computationQueue;
        ComputationQueue.Enqueueing += OnComputationEnqueueing;
    }

    internal ComputationQueue ComputationQueue { get; }

    public HiveThreadPool Run()
    {
        var oldState = Interlocked.CompareExchange(ref _state, (int)PoolState.Running, (int)PoolState.Created);
        
        if (oldState != (int)PoolState.Created)
            throw new InvalidOperationException($"Hive is in \"{oldState}\" state.");
        
        StartMinLiveThreads();

        return this;
    }

    public void Dispose()
    {
        var oldState = Interlocked.Exchange(ref _state, (int)PoolState.Disposed);
        if (oldState == (int)PoolState.Disposed)
            return;

        ComputationQueue.Enqueueing -= OnComputationEnqueueing;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    internal bool RequestFinishingThread(HiveThread thread)
    {
        var finished = RequestFinishingThread();
        if (finished)
            _threads.Remove(thread);

        return finished;
    }

    private void OnComputationEnqueueing()
    {
        if (_state != (int)PoolState.Running)
            return;

        if (RequestStartingNewThread())
            StartNewThread();
    }

    private void StartMinLiveThreads()
    {
        lock (_threadsSyncObject)
        {
            while (_threadsCount < _minLiveThreads)
            {
                _threadsCount++;
                StartNewThread();
            }
        }
    }

    private bool RequestStartingNewThread()
    {
        if (IsDisposed())
            return false;

        lock (_threadsSyncObject)
        {
            if (_threadsCount >= _maxLiveThreads)
                return false;

            var busyThreadsCount = _threads.Count(thread => thread.IsBusy);
            var freeThreadsCount = _threadsCount - busyThreadsCount;

            var mustStart = ComputationQueue.Count + 1 > freeThreadsCount;
            if (mustStart)
                _threadsCount++;

            return mustStart;
        }
    }

    private void StartNewThread()
    {
        var cancellationToken = _cancellationTokenSource.Token;
        var newThread = new HiveThread(this, cancellationToken).Run();

        _threads.Add(newThread);
    }

    private bool RequestFinishingThread()
    {
        if (IsDisposed())
            return true;

        lock (_threadsSyncObject)
        {
            var canFinish = _threadsCount > _minLiveThreads;
            if (canFinish)
                _threadsCount--;
            
            return canFinish;
        }
    }

    private bool IsDisposed() => _state == (int)PoolState.Disposed;

    private enum PoolState
    {
        Created = 1,

        Running,

        Disposed
    }
}