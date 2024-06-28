namespace BeeHive;

public readonly struct Result<TValue>
public record Result<TValue>(
    ResultState State,
    TValue? Value,
    Exception? Error
)
{
    private readonly TValue? _value;
    private readonly Exception? _error;
    private readonly ResultState _state;
    public static Result<TValue> FromValue(TValue? value) =>
        new(State: ResultState.Success, value, Error: default);
    
    public static Result<TValue> FromError(Exception error) =>
        new(State: ResultState.Error, Value: default, Error: error.ArgNotNull(nameof(error)));

    public static Result<TValue> Cancelled() =>
        new(State: ResultState.Cancelled, Value: default, Error: default);
}