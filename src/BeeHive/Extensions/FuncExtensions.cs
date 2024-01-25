namespace BeeHive;

internal static class FuncExtensions
{
    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, TResult> func)
    {
        return (request, _) =>
        {
            var result = func(request);
            return ValueTask.FromResult(result);
        };
    }

    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, Task<TResult>> func)
    {
        return async (request, _) =>
        {
            var result = await func(request);
            return await ValueTask.FromResult(result);
        };
    }

    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, ValueTask<TResult>> func)
    {
        return async (request, _) =>
        {
            var result = await func(request);
            return await ValueTask.FromResult(result);
        };
    }
}