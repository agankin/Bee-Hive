namespace BeeHive;

public class HiveBuilder
{
    private HiveConfiguration _configuration = HiveConfiguration.Default;

    public HiveBuilder WithMinLiveThreads(int minLiveThreads)
    {
        if (minLiveThreads < 1)
            throw new ArgumentException("Minimal threads count cannot be less 1.", nameof(minLiveThreads));

        _configuration = _configuration with { MinLiveThreads = minLiveThreads };
        return this;
    }

    public HiveBuilder WithMaxLiveThreads(int maxLiveThreads)
    {
        if (maxLiveThreads < 1)
            throw new ArgumentException("Maximal threads count cannot be less 1.", nameof(maxLiveThreads));

        _configuration = _configuration with { MaxLiveThreads = maxLiveThreads };
        return this;
    }

    public HiveBuilder WithThreadIdleBeforeStop(int milliseconds)
    {
        _configuration = _configuration with { ThreadIdleBeforeStop = milliseconds };
        return this;
    }

    public Hive Build() => new Hive(_configuration);
}