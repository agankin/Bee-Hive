namespace BeeHive;

internal class HiveThread
{    
    private readonly HiveThreadPool _threadPool;
    private readonly int _idleBeforeStopMilliseconds;
    private readonly CancellationToken _poolCancellationToken;

    private volatile int _isRunning;
    private volatile bool _isBusy;

    public HiveThread(HiveThreadPool threadPool, int idleBeforeStopMilliseconds, CancellationToken poolCancellationToken)
    {
        _threadPool = threadPool;
        _idleBeforeStopMilliseconds = idleBeforeStopMilliseconds;
        _poolCancellationToken = poolCancellationToken;
    }

    public event Action<HiveThread>? ThreadStopped;

    public bool IsRunning => _isRunning > 0;

    public bool IsBusy => _isBusy;

    public HiveThread Run()
    {
        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            throw new InvalidOperationException("Hive Thread is already in running state.");

        var thread = new Thread(QueueHandler)
        {
            IsBackground = true
        };
        thread.Start();

        return this;
    }

    private void QueueHandler()
    {
        SetSynchronizationContext();

        while (true)
        {
            if (_poolCancellationToken.IsCancellationRequested)
                break;

            var hasNext = _threadPool.ComputationQueue.TryTakeOrWait(_idleBeforeStopMilliseconds, _poolCancellationToken, out var next);
            if (hasNext)
            {
                Compute(next);
            }
            else if (RequestFinishing())
            {
                break;
            }
        }

        _isRunning = 0;
        ThreadStopped?.Invoke(this);
    }

    private void Compute(Action? compute)
    {
        try
        {
            _isBusy = true;
            compute?.Invoke();
        }
        catch {}
        finally
        {
            _isBusy = false;
        }
    }

    private void SetSynchronizationContext()
    {
        var ctx = new HiveSynchronizationContext(_threadPool.ComputationQueue);
        SynchronizationContext.SetSynchronizationContext(ctx);
    }

    private bool RequestFinishing() => _threadPool.RequestFinishingThread(this);
}