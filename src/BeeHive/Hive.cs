using System.Collections.Concurrent;

namespace BeeHive;

public class Hive<TRequest, TResult> : IDisposable
{
    private readonly Compute<TRequest, TResult> _compute;
    private readonly HiveThreadPool _threadPool;
    private readonly ConcurrentSet<HiveResultCollection<TResult>> _resultCollections = new();

    public Hive(Compute<TRequest, TResult> compute, HiveConfiguration configuration)
    {
        _compute = compute;
        _threadPool = new HiveThreadPool(configuration).Start();
    }

    public Task<Result<TResult>> EnqueueTask(TRequest request)
    {
        var completionSource = new TaskCompletionSource<Result<TResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var computation = CreateComputation(request, completionSource);
        
        _threadPool.Queue(computation);

        return completionSource.Task;
    }

    public Hive<TRequest, TResult> Start()
    {
        _threadPool.Start();
        return this;
    }

    public void Stop() => _threadPool.Stop();

    public void Dispose() => Stop();

    public BlockingCollection<Result<TResult>> CreateBlockingResults()
    {
        var collection = new HiveResultCollection<TResult>(RemoveDisposedCollection);
        _resultCollections.Add(collection);

        return collection;
    }

    private Action CreateComputation(TRequest request, TaskCompletionSource<Result<TResult>> completionSource)
    {
        void OnResult(Result<TResult> result)
        {
            AddResult(result);
            completionSource.SetResult(result);
        }

        return () =>
        {
            var awaiter = _compute(request, CancellationToken.None).GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                try
                {
                    var resultValue = awaiter.GetResult();
                    var result = Result<TResult>.Value(resultValue);
                    OnResult(result);
                }
                catch (Exception ex)
                {
                    var error = Result<TResult>.Error(ex);
                    OnResult(error);
                }
            });            
        };
    }

    private void AddResult(Result<TResult> result) =>
        _resultCollections.ForEach(collection => collection.Add(result));

    private void RemoveDisposedCollection(HiveResultCollection<TResult> collection) =>
        _resultCollections.Remove(collection);
}