namespace BeeHive;

public record ComputationConfiguration
{
    private ComputationConfiguration() { }

    public static readonly ComputationConfiguration Default = new ComputationConfiguration
    {
        MinLiveThreads = 1,
        MaxLiveThreads = 1
    };

    internal int MinLiveThreads { get; init; }

    internal int MaxLiveThreads { get; init; }
}