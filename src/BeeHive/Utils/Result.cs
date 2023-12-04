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

    private Result(TValue? value, Exception? error, bool hasValue)
    {
        _value = value;
        _error = error;
        _hasValue = hasValue;
    }

    public static Result<TValue> Value(TValue? value) =>
        new Result<TValue>(value, error: default, hasValue: true);

    public static Result<TValue> Error(Exception error) =>
        new Result<TValue>(value: default, error, hasValue: false);

    public TResult Match<TResult>(Func<TValue?, TResult> mapValue, Func<Exception?, TResult> mapError) =>
        _hasValue
            ? mapValue(_value)
            : mapError(_error);

    public Result<TResult> Map<TResult>(Func<TValue?, TResult> mapValue) =>
        _hasValue
            ? Result<TResult>.Value(mapValue(_value))
            : Result<TResult>.Error(_error ?? throw new Exception($"{nameof(_error)} is null."));
}