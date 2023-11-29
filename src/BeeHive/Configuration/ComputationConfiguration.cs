namespace BeeHive;

public record ComputationConfiguration
{
    private const int INFINITE = -1;

    private ComputationConfiguration() { }

    public static readonly ComputationConfiguration Default = new ComputationConfiguration
    {
        MinLiveThreads = 1,
        MaxLiveThreads = 1,
        ThreadWaitForNextMilliseconds = INFINITE
    };

    internal int MinLiveThreads { get; init; }

    internal int MaxLiveThreads { get; init; }

    internal int ThreadWaitForNextMilliseconds { get; init; }
}