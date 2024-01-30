namespace BeeHive;

public readonly struct Result<TValue>
{
    private readonly TValue? _value;
    private readonly Exception? _error;
    private readonly ResultState _state;
    
    public Result()
    {
        _state = ResultState.Success;
        _value = default;
        _error = default;
    }

    private Result(ResultState state, TValue? value, Exception? error)
    {
        _state = ResultState.Success;
        _value = value;
        _error = state == ResultState.Error ? error.ArgNotNull(nameof(error)) : default;
    }

    public static Result<TValue> Value(TValue? value) =>
        new Result<TValue>(state: ResultState.Success, value, error: default);

    public static Result<TValue> Error(Exception error) =>
        new Result<TValue>(state: ResultState.Error, value: default, error.ArgNotNull(nameof(error)));

    public static Result<TValue> Cancelled() =>
        new Result<TValue>(state: ResultState.Cancelled, value: default, error: default);

    public Result<TResult> Map<TResult>(Func<TValue?, TResult> mapValue) =>
        _state switch
        {
            ResultState.Success => Result<TResult>.Value(mapValue(_value)),
            ResultState.Error => Result<TResult>.Error(_error.NotNull("error")),
            ResultState.Cancelled => Result<TResult>.Cancelled(),
            _ => throw GetUnknownState(_state)
        };
    
    public TResult Match<TResult>(
        Func<TValue?, TResult> mapValue,
        Func<Exception, TResult> mapError,
        Func<TResult> mapCancelled)
    {
        var result = _state switch
        {
            ResultState.Success => mapValue(_value),
            ResultState.Error => mapError(_error.NotNull("error")),
            ResultState.Cancelled => mapCancelled(),
            _ => throw GetUnknownState(_state)
        };

        return result;
    }

    public void Match(Action<TValue?> onValue, Action<Exception> onError, Action onCancelled)
    {
        switch (_state)
        {
            case ResultState.Success:
                onValue(_value);
                break;

            case ResultState.Error:
                onError(_error.NotNull());
                break;

            case ResultState.Cancelled:
                onCancelled();
                break;

            default:
                throw GetUnknownState(_state);
        }
    }
    
    private static Exception GetUnknownState(ResultState state) =>
        new Exception($"Unknown {nameof(ResultState)} value: {state}.");
}