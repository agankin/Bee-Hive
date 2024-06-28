namespace BeeHive;

internal static class FuncExtensions
{
    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, TResult> func)
    {
        return (request, _) =>
        {
            var result = func(request);
            return new ValueTask<TResult>(result);
        };
    }

    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, CancellationToken, TResult> func)
    {
        return (request, cancellationToken) =>
        {
            var result = func(request, cancellationToken);
            return new ValueTask<TResult>(result);
        };
    }

    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, Task<TResult>> func)
    {
        return async (request, _) =>
        {
            var result = await func(request);
            return result;
        };
    }

    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, CancellationToken, Task<TResult>> func)
    {
        return async (request, cancellationToken) =>
        {
            var result = await func(request, cancellationToken);
            return result;
        };
    }

    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, ValueTask<TResult>> func)
    {
        return async (request, _) =>
        {
            var result = await func(request);
            return result;
        };
    }

    public static Compute<TRequest, TResult> ToComputeDelegate<TRequest, TResult>(this Func<TRequest, CancellationToken, ValueTask<TResult>> func)
    {
        return async (request, cancellationToken) =>
        {
            var result = await func(request, cancellationToken);
            return result;
        };
    }
}