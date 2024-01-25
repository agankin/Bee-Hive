namespace BeeHive;

public class Hive : IDisposable
{
    private readonly IList<HiveThreadPool> _threadPools = new List<HiveThreadPool>();

    public HiveComputation<TRequest, TResult> AddComputation<TRequest, TResult>(
        Func<TRequest, TResult> computationFunc,
        Func<ComputationConfiguration, ComputationConfiguration>? configure = null)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return AddComputation(computeAsync, configure);
    }

    public HiveComputation<TRequest, TResult> AddComputation<TRequest, TResult>(
        Func<TRequest, Task<TResult>> computationFunc,
        Func<ComputationConfiguration, ComputationConfiguration>? configure = null)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return AddComputation(computeAsync, configure);
    }

    public HiveComputation<TRequest, TResult> AddComputation<TRequest, TResult>(
        Func<TRequest, ValueTask<TResult>> computationFunc,
        Func<ComputationConfiguration, ComputationConfiguration>? configure = null)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return AddComputation(computeAsync, configure);
    }

    public HiveComputation<TRequest, TResult> AddComputation<TRequest, TResult>(
        Compute<TRequest, TResult> compute,
        Func<ComputationConfiguration, ComputationConfiguration>? configure = null)
    {
        var defaultConfig = ComputationConfiguration.Default;
        var config = configure?.Invoke(defaultConfig) ?? defaultConfig;

        var pool = new HiveThreadPool(config).Start();        
        _threadPools.Add(pool);

        return new HiveComputation<TRequest, TResult>(compute, pool);
    }

    public void Dispose()
    {
        _threadPools.ForEach(pool => pool.Stop());
        _threadPools.Clear();
    }
}