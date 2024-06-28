namespace BeeHive;

public static class ResultExtensions
{
    public static Result<TResult> Map<TValue, TResult>(this Result<TValue> result, Func<TValue?, TResult> mapValue)
    {
        var (state, value, error) = result;

        var mappedResult = state switch
        {
            ResultState.Success => Result<TResult>.FromValue(mapValue(value)),
            ResultState.Error => Result<TResult>.FromError(error.NotNull("error")),
            ResultState.Cancelled => Result<TResult>.Cancelled(),
            _ => throw GetUnknownState(state)
        };

        return mappedResult;
    }
    
    public static TResult Match<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue?, TResult> mapValue,
        Func<Exception, TResult> mapError,
        Func<TResult> mapCancelled)
    {
        var (state, value, error) = result;

        var mappedResult = state switch
        {
            ResultState.Success => mapValue(value),
            ResultState.Error => mapError(error.NotNull("error")),
            ResultState.Cancelled => mapCancelled(),
            _ => throw GetUnknownState(state)
        };

        return mappedResult;
    }

    public static void Match<TValue>(
        this Result<TValue> result,
        Action<TValue?> onValue,
        Action<Exception> onError,
        Action onCancelled)
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