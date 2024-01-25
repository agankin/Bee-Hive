namespace BeeHive;

public readonly struct Result<TValue>
{
    private readonly TValue? _value;
    private readonly Exception? _error;
    private readonly bool _hasValue;
    
    public Result()
    {
        _value = default;
        _error = default;
        _hasValue = true;
    }

    private Result(bool hasValue, TValue? value, Exception? error)
    {
        _hasValue = hasValue;
        _value = value;
        _error = _hasValue ? error : error.ArgNotNull(nameof(error));
    }

    public static Result<TValue> Value(TValue? value) =>
        new Result<TValue>(hasValue: true, value, error: default);

    public static Result<TValue> Error(Exception error) =>
        new Result<TValue>(hasValue: false, value: default, error.ArgNotNull(nameof(error)));

    public Result<TResult> Map<TResult>(Func<TValue?, TResult> mapValue) =>
        _hasValue
            ? Result<TResult>.Value(mapValue(_value))
            : Result<TResult>.Error(_error.NotNull("error"));
    
    public TResult Match<TResult>(Func<TValue?, TResult> mapValue, Func<Exception, TResult> mapError) =>
        _hasValue
            ? mapValue(_value)
            : mapError(_error.NotNull("error"));

    public void Match(Action<TValue?> onValue, Action<Exception> onError)
    {
        if (_hasValue)
        {
            onValue(_value);
        }
        else
        {
            onError(_error.NotNull("error"));
        }
    }
}