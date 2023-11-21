using System.Collections.Concurrent;

namespace BeeHive;

public class HiveComputation<TRequest, TResult>
{
    private readonly Func<TRequest, TResult> _compute;
    private readonly HiveThreadPool _threadPool;
    private readonly ConcurrentSet<HiveResultCollection<TResult>> _resultCollections = new();

    internal HiveComputation(Func<TRequest, TResult> compute, HiveThreadPool threadPool)
    {
        _compute = compute;
        _threadPool = threadPool;
    }

    public Task<TResult> Compute(TRequest request)
    {
        var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var computation = CreateComputation(request, completionSource);
        
        _threadPool.Queue(computation);

        return completionSource.Task;
    }

    public BlockingCollection<TResult> GetNewResultCollection()
    {
        var collection = new HiveResultCollection<TResult>(RemoveDisposedCollection);
        _resultCollections.Add(collection);

        return collection;
    }

    private Action CreateComputation(TRequest request, TaskCompletionSource<TResult> completionSource)
    {
        return () =>
        {
            var result = _compute(request);
            AddResult(result);

            completionSource.SetResult(result);
        };
    }

    private void AddResult(TResult result) =>
        _resultCollections.ForEach(collection => collection.Add(result));

    private void RemoveDisposedCollection(HiveResultCollection<TResult> collection) =>
        _resultCollections.Remove(collection);
}