using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal class HiveComputationQueue
{
    private readonly SemaphoreSlim _semaphore = new(0);
    
    private readonly ConcurrentQueue<HiveComputation> _computationQueue = new();
    private readonly ConcurrentQueue<Action> _continuationQueue = new();

    private readonly int _waitForNextMilliseconds;

    public HiveComputationQueue(int waitForNextMilliseconds)
    {
        _waitForNextMilliseconds = waitForNextMilliseconds;
    }

    public event Action? Enqueueing;

    public int Count => _computationQueue.Count + _continuationQueue.Count;

    public void EnqueueComputation(HiveComputation computation)
    {
        Enqueueing?.Invoke();
        
        _computationQueue.Enqueue(computation);
        _semaphore.Release();
    }

    public void EnqueueContinuation(Action continuation)
    {
        Enqueueing?.Invoke();

        _continuationQueue.Enqueue(continuation);
        _semaphore.Release();
    }

    public bool TryDequeueOrWait(Func<bool> canSkipWaitingForNext, CancellationToken cancellationToken, [MaybeNullWhen(false)] out Action next)
    {
        if (TryDequeue(out next))
            return true;

        while (true)
        {
            if (!WaitForNext(cancellationToken) && canSkipWaitingForNext())
                return false;

            if (TryDequeue(out next))
                return true;
        }
    }

    private bool TryDequeue([MaybeNullWhen(false)] out Action action)
    {
        if (_continuationQueue.TryDequeue(out var nextContinuation))
        {
            action = nextContinuation;
            return true;
        }

        if (_computationQueue.TryDequeue(out var nextComputation))
        {
            action = nextComputation.Compute;
            return true;
        }

        action = null;
        return false;
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