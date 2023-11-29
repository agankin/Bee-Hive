using System.Collections.Concurrent;

namespace BeeHive;

internal class BlockingQueue<TValue>
{
    private readonly SemaphoreSlim _semaphore = new(0);
    private readonly ConcurrentQueue<TValue> _queue = new();

    private readonly int _waitForNextMilliseconds;

    public BlockingQueue(int waitForNextMilliseconds) => _waitForNextMilliseconds = waitForNextMilliseconds;

    public void Enqueue(TValue value)
    {
        _queue.Enqueue(value);
        _semaphore.Release();
    }

    public int Count => _queue.Count;

    public DequeueResult<TValue> DequeueOrWait(Func<bool> canSkipWaitingForNext, CancellationToken cancellationToken)
    {
        if (_queue.TryDequeue(out var next))
            return new(true, next);

        while (true)
        {
            if (!WaitForNext(cancellationToken) && canSkipWaitingForNext())
                return new(false, default);

            if (_queue.TryDequeue(out next))
                return new(true, next);
        }
    }

    private bool WaitForNext(CancellationToken cancellationToken)
    {
        try
        {
            return _semaphore.Wait(_waitForNextMilliseconds, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}

public readonly record struct DequeueResult<TValue>(
    bool HasValue,
    TValue? Value
);