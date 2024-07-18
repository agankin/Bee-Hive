namespace BeeHive;

public static class ResultExtensions
{
    public static Result<TRequest, TResult2> Map<TRequest, TResult, TResult2>(this Result<TRequest, TResult> result, Func<TResult?, TResult2> mapValue)
    {
        var (request, state, value, error) = result;

        var mappedResult = state switch
        {
            ResultState.Success => Result<TRequest, TResult2>.FromValue(request, mapValue(value)),
            ResultState.Error => Result<TRequest, TResult2>.FromError(request, error.NotNull("error")),
            ResultState.Cancelled => Result<TRequest, TResult2>.Cancelled(request),
            _ => throw GetUnknownState(state)
        };

        return mappedResult;
    }
    
    public static TResult2 Match<TRequest, TResult, TResult2>(
        this Result<TRequest, TResult> result,
        Func<TResult, TResult2> mapValue,
        Func<Exception, TResult2> mapError,
        Func<TResult2> mapCancelled)
    {
        var (_, state, value, error) = result;

        var mappedResult = state switch
        {
            ResultState.Success => mapValue(value.NotNull("value")),
            ResultState.Error => mapError(error.NotNull("error")),
            ResultState.Cancelled => mapCancelled(),
            _ => throw GetUnknownState(state)
        };

        return mappedResult;
    }

    public static void Match<TRequest, TResult>(this Result<TRequest, TResult> result, Action<TResult> onValue, Action<Exception> onError, Action onCancelled)
    {
        result.Match(onValue.ToFunc(), onError.ToFunc(), onCancelled.ToFunc());
    }
    
    private static Exception GetUnknownState(ResultState state) =>
        new Exception($"Unknown {nameof(ResultState)} value: {state}.");

    private static Func<TValue, Nothing> ToFunc<TValue>(this Action<TValue> action)
    {
        return value =>
        {
            action(value);
            return new Nothing();
        };
    }

    private static Func<Nothing> ToFunc(this Action action)
    {
        return () =>
        {
            action();
            return new Nothing();
        };
    }
}