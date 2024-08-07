﻿namespace BeeHive;

using static DebugLogger;

internal class HiveThread
{    
    private readonly HiveThreadPool _threadPool;
    private readonly int _idleBeforeStopMilliseconds;
    private readonly CancellationToken _poolCancellationToken;

    private int _isRunning;
    private volatile bool _isBusy;

    public HiveThread(HiveThreadPool threadPool, int idleBeforeStopMilliseconds, CancellationToken poolCancellationToken)
    {
        _threadPool = threadPool;
        _idleBeforeStopMilliseconds = idleBeforeStopMilliseconds;
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
        LogDebug("Hive thread started.");
        InitializeSynchronizationContext();

        while (true)
        {
            if (_poolCancellationToken.IsCancellationRequested)
                break;

            var hasNext = _threadPool.ComputationQueue.TryTakeOrWait(_idleBeforeStopMilliseconds, _poolCancellationToken, out var next);
            if (hasNext)
            {
                LogDebug("Invoking computation...");
                Compute(next);
            }
            else if(RequestFinishing())
            {
                break;
            }
        }

        LogDebug("Hive thread stopped.");
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