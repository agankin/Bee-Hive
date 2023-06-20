namespace BeeHive;

internal interface ISchedulingStrategy
{
    void Schedule(IReadOnlyList<HiveThread> threads, Action computation);
}