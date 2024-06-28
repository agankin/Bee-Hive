namespace BeeHive;

public static class Hive
{
    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }

    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, CancellationToken, TResult> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }

    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }

    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, CancellationToken, Task<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }

    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }

    public static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Func<TRequest, CancellationToken, ValueTask<TResult>> computationFunc)
    {
        var computeAsync = computationFunc.ToComputeDelegate();
        return ToCompute(computeAsync);
    }

    private static HiveBuilder<TRequest, TResult> ToCompute<TRequest, TResult>(Compute<TRequest, TResult> compute) =>
        new HiveBuilder<TRequest, TResult>(compute);
}