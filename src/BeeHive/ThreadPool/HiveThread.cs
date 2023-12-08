namespace BeeHive;

using static ConsoleLogger;

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

        var dequeueNext = true;
        while (dequeueNext)
        {
            var requestFinishing = () => _threadPool.RequestFinishingThread(this);
            var (hasValue, computation) = _threadPool.ComputationQueue.DequeueOrWait(requestFinishing, _cancellationToken);
            
            if (hasValue)
            {
                Log("Invoking computation..."); 
                computation?.Invoke();
            }

            dequeueNext = hasValue;
        }

        Log("Hive thread finished.");
    }

    private void InitializeSynchronizationContext()
    {
        var ctx = new HiveSynchronizationContext(_threadPool);
        SynchronizationContext.SetSynchronizationContext(ctx);
    }
}