using System.Collections.Concurrent;

namespace BeeHive;

public class HiveComputation<TRequest, TResult>
{
    private readonly HiveComputationId _id;
    private readonly Func<TRequest, TResult> _compute;
    private readonly Action<HiveComputationId, Action> _queueComputation;
    private readonly ConcurrentSet<HiveResultCollection<TResult>> _resultCollections = new();

    internal HiveComputation(
        HiveComputationId id,
        Func<TRequest, TResult> compute,
        Action<HiveComputationId, Action> queueComputation)
    {
        _id = id;
        _queueComputation = queueComputation;
        _compute = compute;
    }

    public async Task<TResult> RequestAsync(TRequest request)
    {
        var completion = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var computation = CreateComputationForAsyncRequest(request, completion);
        
        _queueComputation(_id, computation);

        return await completion.Task.ConfigureAwait(false);
    }

    public void QueueRequest(TRequest request)
    {
        var computation = CreateComputationForQueueRequest(request);
        _queueComputation(_id, computation);
    }

    public BlockingCollection<TResult> GetNewResultCollection()
    {
        var collection = new HiveResultCollection<TResult>(RemoveDisposedCollection);
        _resultCollections.Add(collection);

        return collection;
    }

    private Action CreateComputationForAsyncRequest(TRequest request, TaskCompletionSource<TResult> taskCompletion)
    {
        return () =>
        {
            var result = _compute(request);
            AddResult(result);

            taskCompletion.SetResult(result);
        };
    }

    private Action CreateComputationForQueueRequest(TRequest request)
    {
        return () =>
        {
            var result = _compute(request);
            AddResult(result);
        };
    }

    private void AddResult(TResult result) =>
        _resultCollections.ForEach(collection => collection.Add(result));

    private void RemoveDisposedCollection(HiveResultCollection<TResult> collection) =>
        _resultCollections.Remove(collection);
}