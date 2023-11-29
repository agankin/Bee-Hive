namespace BeeHive;

public static class ComputationConfigurationBuilder
{
    public static ComputationConfiguration MinLiveThreads(this ComputationConfiguration config, int minLiveThreads) =>
        config with { MinLiveThreads = minLiveThreads };

    public static ComputationConfiguration MaxLiveThreads(this ComputationConfiguration config, int maxLiveThreads) =>
        config with { MaxLiveThreads = maxLiveThreads };

    public static ComputationConfiguration ThreadWaitForNext(this ComputationConfiguration config, int milliseconds) =>
        config with { ThreadWaitForNextMilliseconds = milliseconds };
}