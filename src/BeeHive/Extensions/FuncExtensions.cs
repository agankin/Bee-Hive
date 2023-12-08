namespace BeeHive;

internal static class FuncExtensions
{
    public static AsyncFunc<TRequest, TResult> ToAsyncFunc<TRequest, TResult>(this Func<TRequest, TResult> func)
    {
        return request =>
        {
            var result = func(request);
            return ValueTask.FromResult(result);
        };
    }

    public static AsyncFunc<TRequest, TResult> ToAsyncFunc<TRequest, TResult>(this Func<TRequest, Task<TResult>> func)
    {
        return async request =>
        {
            var result = await func(request);
            return await ValueTask.FromResult(result);
        };
    }
}