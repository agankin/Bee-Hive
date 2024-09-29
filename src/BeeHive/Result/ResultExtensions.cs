namespace BeeHive;

/// <summary>
/// Contains extension methods for <see cref="Result{TRequest, TResult}"/>.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps value from the result by applying a delegate and then wrapps into a new result.
    /// When the result contains no value it returns unchanged copy.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="mapValue">A delegate to map the value.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <typeparam name="TMappedResult">The mapped value type.</typeparam>
    /// <returns>A new instance of <see cref="Result{TRequest, TResult}"/>.</returns>
    public static Result<TRequest, TMappedResult> Map<TRequest, TResult, TMappedResult>(
        this Result<TRequest, TResult> result,
        Func<TResult?, TMappedResult> mapValue)
    {
        var (request, state, value, error) = result;

        var mappedResult = state switch
        {
            ResultState.Success => Result<TRequest, TMappedResult>.FromValue(request, mapValue(value)),
            ResultState.Error => Result<TRequest, TMappedResult>.FromError(request, error.NotNull("error")),
            ResultState.Cancelled => Result<TRequest, TMappedResult>.Cancelled(request),
            _ => throw GetUnknownState(state)
        };

        return mappedResult;
    }
    
    /// <summary>
    /// Matches the result by invoking a corresponding delegate.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="onValue">A delegate to match the value.</param>
    /// <param name="onError">A delegate to match the error.</param>
    /// <param name="onCancelled">A delegate to match cancellation.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <typeparam name="TMatchingResult">The type of matching result.</typeparam>
    /// <returns>A result of matching.</returns>
    public static TMatchingResult Match<TRequest, TResult, TMatchingResult>(
        this Result<TRequest, TResult> result,
        Func<TResult, TMatchingResult> mapValue,
        Func<Exception, TMatchingResult> mapError,
        Func<TMatchingResult> mapCancelled)
    {
        var (_, state, value, error) = result;

        var mappedResult = state switch
        {
            ResultState.Success => mapValue(value!),
            ResultState.Error => mapError(error.NotNull("error")),
            ResultState.Cancelled => mapCancelled(),
            _ => throw GetUnknownState(state)
        };

        return mappedResult;
    }

    /// <summary>
    /// Matches the result by invoking a corresponding delegate.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="onValue">A delegate to match the value.</param>
    /// <param name="onError">A delegate to match the error.</param>
    /// <param name="onCancelled">A delegate to match cancellation.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    public static void Match<TRequest, TResult>(
        this Result<TRequest, TResult> result,
        Action<TResult> onValue,
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