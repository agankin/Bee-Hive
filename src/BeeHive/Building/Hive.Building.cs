namespace BeeHive;

public static class Hive
{
    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Compute<TRequest, TResult> compute) =>
        new HiveBuilder<TRequest, TResult>(compute);

    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }

    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }

    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }
}