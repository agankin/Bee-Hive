namespace BeeHive;

using static DebugLogger;

internal class HiveThread
{    
    private readonly HiveThreadPool _threadPool;
    private readonly CancellationToken _poolCancellationToken;

    private int _isRunning;
    private volatile bool _isBusy;

    public HiveThread(HiveThreadPool threadPool, CancellationToken poolCancellationToken)
    {
        _threadPool = threadPool;
        _poolCancellationToken = poolCancellationToken;
    }

    public bool IsBusy => _isBusy;

    public HiveThread Run()
    {
        var isRunning = Interlocked.CompareExchange(ref _isRunning, 1, 0);
        if (isRunning == 1)
            throw new InvalidOperationException("Hive Thread is already in running state.");

        Task.Factory.StartNew(QueueHandler, TaskCreationOptions.LongRunning);

        return this;
    }

    private void QueueHandler()
    {
        Log("Hive thread started.");
        InitializeSynchronizationContext();

        var goOn = true;
        while (goOn)
        {
            if (_poolCancellationToken.IsCancellationRequested)
                break;

            var dequed = _threadPool.ComputationQueue.TryDequeueOrWait(RequestFinishing, _poolCancellationToken, out var next);
            if (dequed)
            {
                Log("Invoking computation...");
                Compute(next);
            }

            goOn = dequed;
        }

        Log("Hive thread stopped.");
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

    private void InitializeSynchronizationContext()
    {
        var ctx = new HiveSynchronizationContext(_threadPool.ComputationQueue);
        SynchronizationContext.SetSynchronizationContext(ctx);
    }

    private bool RequestFinishing() => _threadPool.RequestFinishingThread(this);
}