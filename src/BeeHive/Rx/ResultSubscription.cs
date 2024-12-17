namespace BeeHive;

internal class ResultSubscription<TRequest, TResult> : IDisposable
{
    private readonly IObserver<Result<TRequest, TResult>> _observer;
    private readonly ResultSubscriptionSet<TRequest, TResult> _subscriptions;

    private volatile int _disposed;

    public ResultSubscription(IObserver<Result<TRequest, TResult>> observer, ResultSubscriptionSet<TRequest, TResult> subscriptions)
    {
        _observer = observer;
        _subscriptions = subscriptions;
    }

    internal void OnNext(Result<TRequest, TResult> result)
    {
        try
        {
            _observer.OnNext(result);
        }
        catch {}
    }

    internal void Complete()
    {
        try
        {
            _observer.OnCompleted();
        }
        catch {}
        
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        var disposed = Interlocked.CompareExchange(ref _disposed, 1, 0);
        if (disposed != 0)
            return;

        _subscriptions.Remove(this);
    }
}