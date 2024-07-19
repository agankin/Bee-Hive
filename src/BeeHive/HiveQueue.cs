using System.Collections;

namespace BeeHive;

public class HiveQueue<TRequest, TResult> : IEnumerable<Computation<TRequest, TResult>>
{
    private readonly ComputationQueue _poolComputationQueue;
    private readonly ComputationFactory<TRequest, TResult> _computationFactory;
    
    private readonly ConcurrentSet<Computation<TRequest, TResult>> _computations = new();
    private readonly Lazy<HiveResultBag<TRequest, TResult>> _resultBag = new(() => new());

    internal HiveQueue(ComputationQueue poolComputationQueue, Compute<TRequest, TResult> compute)
    {
        _poolComputationQueue = poolComputationQueue;
        _computationFactory = new ComputationFactory<TRequest, TResult>(compute, OnComputationCompleted);
    }

    public HiveTask<TRequest, TResult> Compute(TRequest request)
    {
        var (computation, task) = _computationFactory.Create(request);
        _poolComputationQueue.EnqueueComputation(computation.Compute);

        return task;
    }

    public IBlockingReadOnlyCollection<Result<TRequest, TResult>> GetResultBag() => _resultBag.Value;

    public IEnumerator<Computation<TRequest, TResult>> GetEnumerator() => _computations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    private void OnComputationCompleted(Computation<TRequest, TResult> computation)
    {
        _computations.Remove(computation);

        if (_resultBag.IsValueCreated)
            _resultBag.Value.Add(computation.Result);
    }
}