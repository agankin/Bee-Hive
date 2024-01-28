namespace BeeHive;

public static class HiveConfigurationBuilder
{
    public static HiveConfiguration MinLiveThreads(this HiveConfiguration config, int minLiveThreads) =>
        config with { MinLiveThreads = minLiveThreads };

    public static HiveConfiguration MaxLiveThreads(this HiveConfiguration config, int maxLiveThreads) =>
        config with { MaxLiveThreads = maxLiveThreads };

    public static HiveConfiguration ThreadWaitForNext(this HiveConfiguration config, int milliseconds) =>
        config with { ThreadWaitForNextMilliseconds = milliseconds };
}