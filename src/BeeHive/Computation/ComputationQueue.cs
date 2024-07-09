using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal class ComputationQueue
{
    private readonly SemaphoreSlim _semaphore = new(0);
    
    private readonly ConcurrentQueue<Action> _computationQueue = new();
    private readonly ConcurrentQueue<Action> _continuationQueue = new();

    private readonly int _threadIdleBeforeStop;

    public ComputationQueue(int threadIdleBeforeStop)
    {
        _threadIdleBeforeStop = threadIdleBeforeStop;
    }

    public event Action? Enqueueing;

    public int Count => _computationQueue.Count + _continuationQueue.Count;

    public void EnqueueComputation(Action compute)
    {
        Enqueueing?.Invoke();
        
        _computationQueue.Enqueue(compute);
        _semaphore.Release();
    }

    public void EnqueueContinuation(Action continuation)
    {
        Enqueueing?.Invoke();

        _continuationQueue.Enqueue(continuation);
        _semaphore.Release();
    }

    public bool TryDequeueOrWait(Func<bool> canFinish, CancellationToken cancellationToken, [MaybeNullWhen(false)] out Action next)
    {
        if (TryDequeue(out next))
            return true;

        while (true)
        {
            if (!WaitForNext(cancellationToken) && canFinish())
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
            action = nextComputation;
            return true;
        }

        action = null;
        return false;
    }

    private bool WaitForNext(CancellationToken cancellationToken)
    {
        try
        {
            return _semaphore.Wait(_threadIdleBeforeStop, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}