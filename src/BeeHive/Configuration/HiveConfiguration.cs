namespace BeeHive;

public record HiveConfiguration
{
    private const int INFINITE = -1;

    private HiveConfiguration() { }

    public static readonly HiveConfiguration Default = new HiveConfiguration
    {
        MinLiveThreads = 1,
        MaxLiveThreads = 1,
        ThreadWaitForNextMilliseconds = INFINITE
    };

    internal int MinLiveThreads { get; init; }

    internal int MaxLiveThreads { get; init; }

    internal int ThreadWaitForNextMilliseconds { get; init; }
}