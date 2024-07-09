namespace BeeHive;

public static class HiveQueueBuilder
{
    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }

    public static HiveQueue<TRequest, TResult> GetQueueFor<TRequest, TResult>(this Hive hive, Func<TRequest, CancellationToken, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return hive.GetQueueFor(computeAsync);
    }
}