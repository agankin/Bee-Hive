using System.Collections;

namespace BeeHive;

internal class ResultSubscriptionSet<TRequest, TResult> : IEnumerable<ResultSubscription<TRequest, TResult>>
{
    private readonly ConcurrentSet<ResultSubscription<TRequest, TResult>> _subscriptions = new();

    internal ResultSubscription<TRequest, TResult> Add(IObserver<Result<TRequest, TResult>> observer)
    {
        var subscription = new ResultSubscription<TRequest, TResult>(observer, this);
        _subscriptions.Add(subscription);
        
        return subscription;
    }

    internal void Remove(ResultSubscription<TRequest, TResult> subscription) => _subscriptions.Remove(subscription);
    
    /// <inheritdoc/>
    public IEnumerator<ResultSubscription<TRequest, TResult>> GetEnumerator() => _subscriptions.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}