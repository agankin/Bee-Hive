namespace BeeHive;

using static DebugLogger;

internal class HiveThread
{    
    private readonly HiveThreadPool _threadPool;
    private readonly CancellationToken _cancellationToken;

    private bool _isRunning;

    public HiveThread(HiveThreadPool threadPool, CancellationToken cancellationToken)
    {
        _threadPool = threadPool;
        _cancellationToken = cancellationToken;
    }

    public void Run()
    {
        if (_isRunning)
            throw new InvalidOperationException("Hive Thread is already in running state.");

        _isRunning = true;
        Task.Factory.StartNew(QueueHandler, TaskCreationOptions.LongRunning);
    }

    private void QueueHandler()
    {
        Log("Hive thread started.");
        InitializeSynchronizationContext();

        var goOn = true;
        while (goOn)
        {
            var canFinish = () => _threadPool.RequestFinishingThread(this);
            
            var dequedNext = _threadPool.ComputationQueue.TryDequeueOrWait(canFinish, _cancellationToken, out var next);
            if (dequedNext)
            {
                Log("Invoking computation..."); 
                next?.Invoke();
            }

            goOn = dequedNext;
        }

        Log("Hive thread stopped.");
    }

    private void InitializeSynchronizationContext()
    {
        var ctx = new HiveSynchronizationContext(_threadPool.ComputationQueue);
        SynchronizationContext.SetSynchronizationContext(ctx);
    }
}