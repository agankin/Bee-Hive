using System.Collections.Concurrent;

namespace BeeHive;

public class Hive : IDisposable
{
    private readonly ConcurrentDictionary<HiveComputationId, HiveThreadPool> _threadPoolById = new();

    public HiveComputation<TRequest, TResult> AddComputation<TRequest, TResult>(
        Func<TRequest, TResult> compute,
        Func<ComputationConfiguration, ComputationConfiguration> configure)
    {
        var defaultConfig = ComputationConfiguration.Default;
        var config = configure?.Invoke(defaultConfig) ?? defaultConfig;

        var id = HiveComputationId.Create();
        var pool = new HiveThreadPool(config).Start();

        _threadPoolById.TryAdd(id, pool);

        return new HiveComputation<TRequest, TResult>(id, compute, QueueComputation);
    }

    public void Dispose()
    {
        _threadPoolById.Values.ForEach(pool => pool.Stop());
        _threadPoolById.Clear();
    }

    private void QueueComputation(HiveComputationId id, Action compute)
    {
        if (!_threadPoolById.TryGetValue(id, out var pool))
            throw new InvalidOperationException("Hive Thread Pool not found.");

        pool.Load(compute);
    }
}