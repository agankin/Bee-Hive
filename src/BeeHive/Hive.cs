namespace BeeHive;

/// <summary>
/// Represents a thread pool for computations parallelization.
/// </summary>
public class Hive : IDisposable, IAsyncDisposable
{
    private readonly ComputationQueue _computationQueue;
    private readonly HiveThreadPool _threadPool;

    internal Hive(HiveConfiguration configuration)
    {
        _computationQueue = new ComputationQueue();
        _threadPool = new HiveThreadPool(configuration, _computationQueue);
    }

    /// <summary>
    /// Runs the Hive.
    /// </summary>
    /// <returns>The current instance.</returns>
    public Hive Run()
    {
        _threadPool.Run();
        return this;
    }

    /// <summary>
    /// Creates a Hive Queue for the Hive.
    /// </summary>
    /// <param name="computationFunc">A computation delegate.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>An instance of <see cref="HiveQueue{TRequest, TResult}"/>.</returns>
    public HiveQueue<TRequest, TResult> CreateQueueFor<TRequest, TResult>(Compute<TRequest, TResult> compute) =>
        new HiveQueue<TRequest, TResult>(_computationQueue, compute, _threadPool.CancellationToken);

    /// <inheritdoc/>
    public void Dispose() => _threadPool.Dispose();

    /// <inheritdoc/>
    public async ValueTask DisposeAsync() => await _threadPool.DisposeAsync();
}