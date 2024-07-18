namespace BeeHive;

internal record HiveConfiguration
{
    private const int Infinite = -1;

    private HiveConfiguration() { }

    public static readonly HiveConfiguration Default = new HiveConfiguration
    {
        MinLiveThreads = 1,
        MaxLiveThreads = 1,
        ThreadIdleBeforeStopMilliseconds = Infinite
    };

    internal int MinLiveThreads { get; init; }

    internal int MaxLiveThreads { get; init; }

    internal int ThreadIdleBeforeStopMilliseconds { get; init; }
}