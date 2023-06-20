namespace BeeHive;

internal class HiveThread
{
    private readonly HiveThreadComputationsQueue _computationsQueue = new();
    
    private readonly object _schedulingLock;
    private readonly Func<HiveThread, bool> _onThreadFinishing;

    private bool _isRunning;

    public HiveThread(object schedulingLock, Func<HiveThread, bool> onThreadFinishing)
    {
        _schedulingLock = schedulingLock;
        _onThreadFinishing = onThreadFinishing;
    }

    public int QueuedCount => _computationsQueue.Count;

    public void Load(Action computation) => _computationsQueue.Add(computation);

    public void Run()
    {
        if (_isRunning)
            throw new InvalidOperationException("Hive Thread is already in running state.");

        _isRunning = true;
        Task.Factory.StartNew(QueueHandler, TaskCreationOptions.LongRunning);
    }

    private void QueueHandler()
    {
        DebugLogger.Log("Thread started");
        while (TryGetNext(out var computation))
        {
            computation?.Invoke();
        }
    }

    private bool TryGetNext(out Action? computation)
    {
        TryGetNextComputation tryGetNext = null!;
        lock (_schedulingLock)
        {
            if (_computationsQueue.TryTake(out computation))
                return true;

            tryGetNext = _onThreadFinishing(this) ? FinishQueueHandling : WaitForNewComputation;
        }

        return tryGetNext(out computation);
    }

    private bool WaitForNewComputation(out Action? computation)
    {
        computation = _computationsQueue.Take();
        return true;
    }

    private bool FinishQueueHandling(out Action? computation)
    {
        DebugLogger.Log("Thread finished");

        computation = null;
        return _isRunning = false;
    }

    private delegate bool TryGetNextComputation(out Action? computation);
}