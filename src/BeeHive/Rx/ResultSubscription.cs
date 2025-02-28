namespace BeeHive;

internal class ResultSubscription<TRequest, TResult> : IDisposable
{
    private readonly IObserver<Result<TRequest, TResult>> _observer;
    private readonly ResultSubscriptionSet<TRequest, TResult> _subscriptions;

    private volatile int _completed;
    private volatile int _disposed;

    public ResultSubscription(IObserver<Result<TRequest, TResult>> observer, ResultSubscriptionSet<TRequest, TResult> subscriptions)
    {
        _observer = observer;
        _subscriptions = subscriptions;
    }

    internal void OnNext(Result<TRequest, TResult> result)
    {
        if (_completed != 0 || _disposed != 0)
            return;

        try
        {
            _observer.OnNext(result);
        }
        catch {}
    }

    internal void Complete()
    {
        if (_disposed != 0)
            return;

        if (Interlocked.Exchange(ref _completed, 1) != 0)
            return;

        try
        {
            _observer.OnCompleted();
        }
        catch {}
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Complete();

        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _subscriptions.Remove(this);
    }
}