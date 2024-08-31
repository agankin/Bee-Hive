namespace BeeHive;

/// <summary>
/// A builder for configuring and building Hive instances.
/// </summary>
public class HiveBuilder
{
    private const int InfiniteTime = -1;

    private HiveConfiguration _configuration = HiveConfiguration.Default;

    /// <summary>
    /// Sets lower limit of threads count.
    /// </summary>
    /// <remarks>
    /// By default the value is 1.
    /// </remarks>
    /// <param name="minLiveThreads">A value of the lower limit of threads count.</param>
    /// <returns>The current instance.</returns>
    public HiveBuilder WithMinLiveThreads(int minLiveThreads)
    {
        if (minLiveThreads < 0)
            throw new ArgumentException("Lower limit of threads count cannot be less zero.", nameof(minLiveThreads));

        _configuration = _configuration with { MinLiveThreads = minLiveThreads };
        return this;
    }

    /// <summary>
    /// Sets upper limit of threads count.
    /// </summary>
    /// <remarks>
    /// By default the value is 1.
    /// </remarks>
    /// <param name="maxLiveThreads">A value of the upper limit of threads count.</param>
    /// <returns>The current instance.</returns>
    public HiveBuilder WithMaxLiveThreads(int maxLiveThreads)
    {
        if (maxLiveThreads < 1)
            throw new ArgumentException("Upper limit of threads count cannot be less 1.", nameof(maxLiveThreads));

        _configuration = _configuration with { MaxLiveThreads = maxLiveThreads };
        return this;
    }

    /// <summary>
    /// Sets maximum time a thread can be idle before stopping.
    /// </summary>
    /// <remarks>
    /// By default the idle time is infinite.
    /// Passing a negative value is not allowed except -1 meaning infinite time.
    /// </remarks>
    /// <param name="milliseconds">Idle time in milliseconds.</param>
    /// <returns>The current instance.</returns>
    public HiveBuilder WithThreadIdleBeforeStop(int milliseconds)
    {
        if (milliseconds < 0 && milliseconds != InfiniteTime)
            throw new ArgumentException("The value of maximum time a thread can be idle before stopping must be non-negative or -1 (infinite).", nameof(milliseconds));

        _configuration = _configuration with { ThreadIdleBeforeStopMilliseconds = milliseconds };
        return this;
    }

    /// <summary>
    /// Builds a Hive.
    /// </summary>
    /// <returns>A new instance of Hive.</returns>
    public Hive Build() => new Hive(_configuration);
}