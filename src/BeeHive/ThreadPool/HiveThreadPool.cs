namespace BeeHive;

internal class HiveThreadPool : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentSet<HiveThread> _threads = new();

    private readonly int _minLiveThreads;
    private readonly int _maxLiveThreads;
    private readonly int _threadIdleBeforeStopMilliseconds;
    
    private readonly CancellationTokenSource _poolCancellationTokenSource;
    private readonly CancellationToken _poolCancellationToken;

    private readonly TaskCompletionSource<Nothing> _disposedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    
    private volatile int _state = (int)PoolState.Created;
    private volatile int _threadsCount;

    public HiveThreadPool(HiveConfiguration configuration, ComputationQueue computationQueue)
    {
        _minLiveThreads = configuration.MinLiveThreads;
        _maxLiveThreads = Math.Max(_minLiveThreads, configuration.MaxLiveThreads);
        _threadIdleBeforeStopMilliseconds = configuration.ThreadIdleBeforeStopMilliseconds;

        _poolCancellationTokenSource = new();
        _poolCancellationToken = _poolCancellationTokenSource.Token;

        ComputationQueue = computationQueue;
        ComputationQueue.Enqueueing += OnComputationEnqueueing;
    }

    internal ComputationQueue ComputationQueue { get; }

    internal CancellationToken CancellationToken => _poolCancellationToken;

    public HiveThreadPool Run()
    {
        var oldState = Interlocked.CompareExchange(ref _state, (int)PoolState.Running, (int)PoolState.Created);
        
        if (oldState != (int)PoolState.Created)
            throw new InvalidOperationException($"Hive is in \"{oldState}\" state.");
        
        StartMinLiveThreads();

        return this;
    }

    public void Dispose() => TryDispose();

    public ValueTask DisposeAsync()
    {
        if (TryDispose() && _threads.Count == 0)
            _disposedTaskCompletionSource.TrySetResult(new());

        return new ValueTask(_disposedTaskCompletionSource.Task);
    }

    internal bool RequestFinishingThread(HiveThread thread)
    {
        var finished = RequestFinishingThread();
        if (finished)
        {
            thread.ThreadStopped -= OnThreadStopped;
            _threads.Remove(thread);
        }

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
        var threadsCount = _threadsCount;
        while (threadsCount < _minLiveThreads)
        {
            if (Interlocked.CompareExchange(ref _threadsCount, threadsCount + 1, threadsCount) == threadsCount)
                StartNewThread();
                
            threadsCount = _threadsCount;
        }
    }

    private void StartNewThread()
    {
        var newThread = new HiveThread(this, _threadIdleBeforeStopMilliseconds, _poolCancellationToken).Run();
        newThread.ThreadStopped += OnThreadStopped;
        
        _threads.Add(newThread);
    }

    private bool RequestStartingNewThread()
    {
        if (IsDisposed())
            return false;

        if (_threadsCount >= _maxLiveThreads)
            return false;

        var threadsCount = _threadsCount;
        while (true)
        {
            var busyThreadsCount = _threads.Count(thread => thread.IsBusy);
            var freeThreadsCount = threadsCount - busyThreadsCount;

            var newThreadRequired = ComputationQueue.Count > freeThreadsCount;
            if (!newThreadRequired)
                return false;

            if (Interlocked.CompareExchange(ref _threadsCount, threadsCount + 1, threadsCount) == threadsCount)
                return true;

            threadsCount = _threadsCount;
        }
    }

    private bool RequestFinishingThread()
    {
        if (IsDisposed())
            return true;

        var threadsCount = _threadsCount;
        while (true)
        {
            var threadFinishingRequired = threadsCount > _minLiveThreads;
            if (!threadFinishingRequired)
                return false;
            
            if (Interlocked.CompareExchange(ref _threadsCount, threadsCount - 1, threadsCount) == threadsCount)
                return true;

            threadsCount = _threadsCount;
        }
    }

    private bool TryDispose()
    {
        var oldState = Interlocked.Exchange(ref _state, (int)PoolState.Disposed);
        if (oldState == (int)PoolState.Disposed)
            return false;

        ComputationQueue.Enqueueing -= OnComputationEnqueueing;

        _poolCancellationTokenSource.Cancel();
        _poolCancellationTokenSource.Dispose();

        return true;
    }

    private bool IsDisposed() => _state == (int)PoolState.Disposed;

    private void OnThreadStopped(HiveThread thread)
    {
        if (!IsDisposed())
            return;

        thread.ThreadStopped -= OnThreadStopped;
        _threads.Remove(thread);
        
        if (_threads.Count == 0)
            _disposedTaskCompletionSource.TrySetResult(new());
    }

    private enum PoolState
    {
        Created = 1,

        Running,

        Disposed
    }
}