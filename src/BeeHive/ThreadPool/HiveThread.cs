namespace BeeHive;

internal class HiveThread
{    
    private readonly ComputationQueue _computationQueue;
    private readonly CancellationToken _cancellationToken;
    private readonly Func<HiveThread, bool> _onThreadFinishing;

    private bool _isRunning;

    public HiveThread(ComputationQueue computationQueue, CancellationToken cancellationToken, Func<HiveThread, bool> onThreadFinishing)
    {
        _computationQueue = computationQueue;
        _cancellationToken = cancellationToken;
        _onThreadFinishing = onThreadFinishing;
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
        while (_computationQueue.TryDequeue(out var computation, _cancellationToken))
            computation();
    }
}