namespace BeeHive;

internal class HiveThread
{    
    private readonly BlockingQueue<Action> _computationQueue;
    private readonly CancellationToken _cancellationToken;
    private readonly Func<HiveThread, bool> _requestFinishing;

    private bool _isRunning;

    public HiveThread(BlockingQueue<Action> computationQueue, Func<HiveThread, bool> requestFinishing, CancellationToken cancellationToken)
    {
        _computationQueue = computationQueue;
        _cancellationToken = cancellationToken;
        _requestFinishing = requestFinishing;
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
        var dequeueNext = true;
        while (dequeueNext)
        {
            var (hasValue, computation) = _computationQueue.DequeueOrWait(() => _requestFinishing(this), _cancellationToken);
            
            if (hasValue)
                computation?.Invoke();

            dequeueNext = hasValue;
        }
    }
}