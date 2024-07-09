namespace BeeHive;

public class HiveBuilder<TRequest, TResult>
{
    private readonly Compute<TRequest, TResult> _compute;
    private HiveConfiguration _configuration = HiveConfiguration.Default;

    public HiveBuilder(Compute<TRequest, TResult> compute) => _compute = compute;

    public HiveBuilder<TRequest, TResult> WithMinLiveThreads(int minLiveThreads)
    {
        if (minLiveThreads < 1)
            throw new ArgumentException("Minimal threads count cannot be less 1.", nameof(minLiveThreads));

        _configuration = _configuration with { MinLiveThreads = minLiveThreads };
        return this;
    }

    public HiveBuilder<TRequest, TResult> WithMaxLiveThreads(int maxLiveThreads)
    {
        if (maxLiveThreads < 1)
            throw new ArgumentException("Maximal threads count cannot be less 1.", nameof(maxLiveThreads));

        _configuration = _configuration with { MaxLiveThreads = maxLiveThreads };
        return this;
    }

    public HiveBuilder<TRequest, TResult> WithThreadIdleBeforeStop(int milliseconds)
    {
        _configuration = _configuration with { ThreadIdleBeforeStop = milliseconds };
        return this;
    }

    public Hive<TRequest, TResult> Build() => new Hive<TRequest, TResult>(_compute, _configuration);
}